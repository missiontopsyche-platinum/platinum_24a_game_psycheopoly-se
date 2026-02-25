using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace AIBehavior
{
    public class AIMortgageBehavior
    {
        private readonly Player player;
        private readonly MortgageThresholds thresholds;

        public AIMortgageBehavior(Player player, MortgageThresholds thresholds)
        {
            this.player = player;
            this.thresholds = thresholds;
        }
        
        public (List<OwnableSpaceData> mortgagePoolProperties, List<PropertySpaceData> sellPoolProperties) 
            TestBuildCandidatePools(List<OwnableSpaceData> ownables)
        {
            var (mPool, sPool) = BuildCandidatePools(ownables);
            return (
                mPool.Keys.ToList(), 
                sPool.Values
                    .SelectMany(proxies => proxies
                        .Select(p => p.property))
                    .ToList()
            );
        }

        public float TestCalculateScore(OwnableSpaceData ownable) =>
            CalculateScore(ownable, GetIncomeFromOwnable(ownable));

        public AIMortgageEvaluation EvaluateMortgage()
        {
            int projectedCash = player.GetMoney();

            if (projectedCash >= thresholds.dangerThreshold)
            {
                return new AIMortgageEvaluation(
                    new Queue<MortgageAction>(),
                    "Above the danger threshold, no mortgage necessary.");
            }
            
            var unmortgagedOwnables = player.GetUnmortgagedProperties();
            var (mortgagePool, sellPool) = BuildCandidatePools(unmortgagedOwnables);
            var (actions, message) = SelectMortgageActions(mortgagePool, sellPool, projectedCash);
            
            return new AIMortgageEvaluation(actions, message);
        }

        private (Dictionary<OwnableSpaceData, float>, Dictionary<Color, List<PropertyProxy>>) BuildCandidatePools(
            List<OwnableSpaceData> unmortgagedOwnables)
        {
            var mortgagePool = new Dictionary<OwnableSpaceData, float>();
            var sellPool = new Dictionary<Color, List<PropertyProxy>>();

            // first phase is splitting ownables by type- properties need more checks,
            // but instruments and planets can always be mortgaged if they aren't already.
            foreach (var ownable in unmortgagedOwnables)
            {
                if (ownable is PropertySpaceData property)
                {
                    // initially we add all properties to the sell pool
                    // in order to group them by group color so that we can
                    // evaluate monopolies.
                    if (!sellPool.ContainsKey(property.groupColor))
                        sellPool[property.groupColor] = new List<PropertyProxy>();
                    
                    sellPool[property.groupColor].Add(new PropertyProxy(property));
                }
                else
                {
                    mortgagePool.Add(ownable, 0f);
                }
            }

            // second phase is to interrogate every group/property to see if they're:
            // 1. in a monopoly at all
            // 2. if they have upgrades
            // if either is false, then it gets moved to the mortgage pool and removed from the sell pool
            // this allows us to make sure that unupgraded properties within a group with upgraded
            // properties aren't accidentally mortgaged.
            var groupsToRemove = new List<Color>();
            
            foreach (var (colorGroup, proxies) in sellPool)
            {
                bool isCompleteGroup = proxies.Count == proxies[0].property.numberOfPropertiesInGroup;
                bool hasUpgrades = proxies.Any(p => p.currentUpgradeLevel > 0);

                if (!isCompleteGroup || !hasUpgrades)
                {
                    foreach(var proxy in proxies)
                        mortgagePool.Add(proxy.property, 0f);
                    groupsToRemove.Add(colorGroup);
                }
            }
            // doing this separately to not remove things from an iterating structure
            foreach (var color in groupsToRemove)
                sellPool.Remove(color);

            return (mortgagePool, sellPool);
        }

        private (Queue<MortgageAction>, string) SelectMortgageActions(
            Dictionary<OwnableSpaceData, float> mortgagePool,
            Dictionary<Color, List<PropertyProxy>> sellPool,
            int projectedCash)
        {
            var actions = new Queue<MortgageAction>();

            while (projectedCash < thresholds.dangerThreshold)
            {
                if (mortgagePool.Count == 0 && sellPool.Count == 0)
                {
                    return (actions, projectedCash <= 0 ? "Bankrupt" : "Below Danger Threshold");
                }

                ScoreMortgageCandidates(mortgagePool);
                ScoreSellCandidates(sellPool);

                var action = SelectBestCandidate(mortgagePool, sellPool);
                actions.Enqueue(action);
                projectedCash += action.expectedCashGain;

                UpdatePools(action, mortgagePool, sellPool);
            }

            return (actions, "Recovery successful");
        }

        private float CalculateScore(OwnableSpaceData ownable, int currentIncome)
        {
            var (cashRaised, recoveryCost) = ownable switch
            {
                PropertySpaceData property when property.GetCurrentUpgradeLevel() == 5
                    => (property.dataPointCost * 5 / 2, property.dataPointCost * 5),
                PropertySpaceData property when property.GetCurrentUpgradeLevel() > 0
                    => (property.dataPointCost / 2, property.dataPointCost),
                _ => (ownable.collaborationValue, ownable.collaborationValue * 1.1f)
            };
            var totalCost = recoveryCost + currentIncome;
            return (float)Math.Round(cashRaised / totalCost, 3);
        }

        private void ScoreMortgageCandidates(Dictionary<OwnableSpaceData, float> mortgagePool)
        {
            foreach (var ownable in mortgagePool.Keys.ToList())
            {
                var income = GetIncomeFromOwnable(ownable);
                mortgagePool[ownable] = CalculateScore(ownable, income);
            }
        }

        private int GetIncomeFromOwnable(OwnableSpaceData ownable)
        {
            return ownable switch
            {
                PropertySpaceData property when property.GetCurrentUpgradeLevel() == 5 => 
                    property.researchFundingValues[5] - property.researchFundingValues[0],
                PropertySpaceData property when property.GetCurrentUpgradeLevel() > 0 => 
                    property.researchFundingValues[property.GetCurrentUpgradeLevel()] 
                    - property.researchFundingValues[property.GetCurrentUpgradeLevel() - 1],
                PropertySpaceData property when property.GetCurrentUpgradeLevel() == 0 => 
                    property.researchFundingValues[0],
                InstrumentSpaceData instrument => 
                    instrument.researchFundingLevels[player.GetNumberInstrumentsOwned() - 1],
                PlanetSpaceData planet => 
                    planet.diceMultipliers[player.GetNumberPlanetsOwned() - 1] * 7,
                _ => 0
            };
        }

        private void ScoreSellCandidates(Dictionary<Color, List<PropertyProxy>> sellPool)
        {
            foreach (var proxies in sellPool.Values)
            {
                foreach (var proxy in proxies)
                    proxy.currentScore = 0;
                
                var localMax = proxies.Max(p => p.currentUpgradeLevel);
                foreach (var proxy in proxies.Where(p => p.currentUpgradeLevel == localMax))
                {
                    int income = proxy.GetRentAtLevel(localMax) -
                                 proxy.GetRentAtLevel(localMax - 1);
                    proxy.currentScore = CalculateScore(proxy.property, income);
                }
            }
        }

        private MortgageAction SelectBestCandidate(
            Dictionary<OwnableSpaceData, float> mortgagePool, 
            Dictionary<Color, List<PropertyProxy>> sellPool)
        {
            var bestMortgage = mortgagePool.Count > 0
                ? mortgagePool
                    .OrderByDescending(kvp => kvp.Value)
                    .ThenByDescending(kvp => kvp.Key.collaborationValue)
                    .ThenBy(kvp => GetIncomeFromOwnable(kvp.Key))
                    .First().Key
                : null;

            var bestSell = sellPool.Count > 0
                ? sellPool.Values
                    .SelectMany(proxies => proxies.Where(p => p.currentScore > 0))
                    .OrderByDescending(p => p.currentScore)
                    .ThenByDescending(p => p.property.collaborationValue)
                    .ThenBy(p => GetIncomeFromOwnable(p.property))
                    .First()
                : null;

            // defensive, might be overkill.
            if (bestMortgage == null && bestSell == null) 
                return null;
            if (bestMortgage == null && bestSell != null) 
                return BuildMortgageAction(bestSell.property, bestSell.currentUpgradeLevel);
            
            if (bestSell == null || mortgagePool[bestMortgage] >= bestSell.currentScore)
                return BuildMortgageAction(bestMortgage);

            return BuildMortgageAction(bestSell.property, bestSell.currentUpgradeLevel);
        }

        private MortgageAction BuildMortgageAction(OwnableSpaceData winner, int proxyLevel = 0)
        {
            return winner switch
            {
                PropertySpaceData property => BuildPropertyAction(property, proxyLevel),
                _ => new MortgageAction(winner, MortgageActionType.Mortgage, winner.collaborationValue)
            };
        }

        private MortgageAction BuildPropertyAction(PropertySpaceData property, int proxyLevel)
        {
            return proxyLevel switch
            {
                5 => new MortgageAction(property, MortgageActionType.SellDiscovery, property.dataPointCost * 5 / 2),
                > 0 => new MortgageAction(property, MortgageActionType.SellDataPoint, property.dataPointCost / 2),
                _ => new MortgageAction(property, MortgageActionType.Mortgage, property.collaborationValue)
            };
        }

        private void UpdatePools(
            MortgageAction action, 
            Dictionary<OwnableSpaceData, float> mortgagePool, 
            Dictionary<Color, List<PropertyProxy>> sellPool)
        {
            // if its a mortgage we can just remove it from the pool, easy peasy
            if (action.actionType == MortgageActionType.Mortgage)
            {
                mortgagePool.Remove(action.ownableSpace);
                return;
            }

            // sell action. we need to find the right proxy and update their 'state'
            var groupProxies = sellPool[action.ownableSpace.groupColor];
            var proxy = groupProxies.First(p => p.property == action.ownableSpace);
            proxy.currentUpgradeLevel--;

            // check the new local max upgrade for the prop group
            var newMax = groupProxies.Max(p => p.currentUpgradeLevel);

            // if all props in group have no upgrades, we can move it to the mortgage pool
            if (newMax == 0)
            {
                foreach (var p in groupProxies)
                    mortgagePool[p.property] = 0f;
                
                sellPool.Remove(action.ownableSpace.groupColor);
            }
        }

        private class PropertyProxy
        {
            public PropertySpaceData property { get; }
            public int currentUpgradeLevel { get; set; }
            public float currentScore { get; set; }

            public PropertyProxy(PropertySpaceData property)
            {
                this.property = property;
                currentUpgradeLevel = this.property.GetCurrentUpgradeLevel();
                currentScore = 0f;
            }

            public int GetRentAtLevel(int level)
            {
                return property.researchFundingValues[level];
            }
        }
    }
    
    public class AIMortgageEvaluation
    {
        public Queue<MortgageAction> actions { get; }
        public string message { get; }

        public AIMortgageEvaluation(Queue<MortgageAction> actions, string message)
        {
            this.actions = actions;
            this.message = message;
        }
    }
}