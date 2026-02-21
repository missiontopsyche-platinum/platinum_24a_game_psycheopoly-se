using System.Collections.Generic;
using System.Linq;
using Data;
using Logging;
using UnityEngine;
using Logger = Logging.Logger;

namespace AIBehavior
{
    public class AIUpgradeBehavior
    {
        private readonly Player player;
        private readonly UpgradeWeights weights;
        private readonly UpgradeThresholds thresholds;

        private readonly float sigmoidMin;
        private readonly float sigmoidMax;
        /// <summary>
        /// This controls the steepness of the curve in the Sigmoid function. Lower numers are more shallow,
        /// while higher numbers are steeper. We could tune this if we'd like.
        /// </summary>
        private const float SigmoidK = 10f;
        /// <summary>
        /// This controls the center-point of the sigmoid function. Since our uses are mapped from 0.0 -> 1.0,
        /// we want to keep this somewhat in the middle. 0.5f is dead center, but 0.6 and 0.7 have useful
        /// outputs as well. We could tune this if we'd like.
        /// </summary>
        private const float SigmoidC = 0.5f;

        public AIUpgradeBehavior(Player player, UpgradeWeights upgradeWeights, UpgradeThresholds upgradeThresholds)
        {
            this.player = player;
            weights = upgradeWeights;
            thresholds = upgradeThresholds;

            sigmoidMin = Sigmoid(0f);
            sigmoidMax = Sigmoid(1f);
        }

        public float TestCalculateScore(PropertySpaceData property) => CalculateScore(property);

        public AIUpgradeEvaluation EvaluateUpgrade()
        {
            List<PropertySpaceData> eligibleProperties = player.GetValidUpgradableProperties();
            Dictionary<PropertySpaceData, float> passingProperties = EvaluateEligibleProperties(eligibleProperties);

            if (passingProperties.Count == 0)
                return AIUpgradeEvaluation.DeferUpgrade();

            KeyValuePair<PropertySpaceData, float> best = FindBestProperty(passingProperties);
            
            Logger.Info("AIUpgradeBehavior.EvaluateUpgrade",
                $"Decided to upgrade {best.Key.spaceName} with an evaluation score of {best.Value}",
                LogCategory.AI);

            return AIUpgradeEvaluation.FoundUpgradeTarget(best.Key);
        }

        private Dictionary<PropertySpaceData, float> EvaluateEligibleProperties(List<PropertySpaceData> eligibleProperties)
        {
            Dictionary<PropertySpaceData, float> passingProperties = new();

            foreach (PropertySpaceData property in eligibleProperties)
            {
                if (!CanAffordUpgrade(property))
                    continue;
                
                float score = CalculateScore(property);
                float threshold = CalculateThreshold();
                
                if (score >= threshold)
                    passingProperties.Add(property, score);
            }
            return passingProperties;
        }

        private float CalculateScore(PropertySpaceData property)
        {
            int currentUpgradeLevel = property.GetCurrentUpgradeLevel();
            float score = 0f;

            score += weights.baseUpgradeScore * weights.upgradeLevelWeight[currentUpgradeLevel];
            score += CalculateNormalizedROI(property, currentUpgradeLevel) * weights.roiWeight;
            score += CalculateNormalizedReserveSigmoid(property) * weights.reserveCushionWeight;
            
            return score;
        }

        private float CalculateNormalizedROI(PropertySpaceData property, int upgradeLevel)
        {
            float maxDelta = CalculateMaxRentDelta(property);
            float rentDelta = property.researchFundingValues[upgradeLevel + 1] -
                              property.researchFundingValues[upgradeLevel];
            return rentDelta / maxDelta;
        }

        private float CalculateMaxRentDelta(PropertySpaceData property)
        {
            float maxDelta = 0f;
            for (int i = 0; i < property.researchFundingValues.Length - 1; i++)
            {
                float delta = property.researchFundingValues[i + 1] - property.researchFundingValues[i];
                if (delta > maxDelta)
                    maxDelta = delta;
            }
            return maxDelta;
        }

        private float CalculateNormalizedReserveSigmoid(PropertySpaceData property)
        {
            float reserveRatio = (float)(player.GetMoney() - property.dataPointCost) / thresholds.startingCash;
            float sigmoidValue = Sigmoid(reserveRatio);

            // normalize the results of the function
            return (sigmoidValue - sigmoidMin) / (sigmoidMax - sigmoidMin);
        }

        private float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-SigmoidK * (x - SigmoidC)));

        private float CalculateThreshold()
        {
            float variance = Random.Range(-thresholds.randomVariance, thresholds.randomVariance);
            return thresholds.baseThreshold + variance;
        }

        private bool CanAffordUpgrade(PropertySpaceData property)
        {
            return thresholds.minimumReserve <= (player.GetMoney() - property.dataPointCost);
        }

        private KeyValuePair<PropertySpaceData, float> FindBestProperty(Dictionary<PropertySpaceData, float> passingProperties)
        {
            var best = passingProperties.OrderByDescending(kvp => kvp.Value).First();
            return best;
        }
    }

    public class AIUpgradeEvaluation
    {
        public bool willUpgrade { get; }
        public PropertySpaceData upgradeTarget { get; }

        private AIUpgradeEvaluation(bool willUpgrade, PropertySpaceData upgradeTarget)
        {
            this.willUpgrade = willUpgrade;
            this.upgradeTarget = upgradeTarget;
        }
        
        public static AIUpgradeEvaluation FoundUpgradeTarget(PropertySpaceData upgradeTarget)
        {
            return new AIUpgradeEvaluation(true, upgradeTarget);
        }

        public static AIUpgradeEvaluation DeferUpgrade()
        {
            return new AIUpgradeEvaluation(false, null);
        }
    }
}