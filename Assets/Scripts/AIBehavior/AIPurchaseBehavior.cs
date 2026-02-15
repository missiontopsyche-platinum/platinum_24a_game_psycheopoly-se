using UnityEngine;
using Data;
using Logging;
using Random = UnityEngine.Random;

namespace AIBehavior
{
    public class AIPurchaseBehavior
    {
        private readonly Player player;
        private readonly PurchaseWeights purchaseWeights;
        private readonly PurchaseThresholds purchaseThresholds;

        public AIPurchaseBehavior(Player player, PurchaseWeights purchaseWeights, PurchaseThresholds purchaseThresholds)
        {
            this.player = player;
            this.purchaseWeights = purchaseWeights;
            this.purchaseThresholds = purchaseThresholds;
        }
        
        public bool EvaluatePurchase(OwnableSpaceData ownableSpace)
        {
            float score = CalculatePurchaseScore(ownableSpace);
            float threshold = CalculateThreshold();

            bool decision = score >= threshold;
            
            Logging.Logger.Info("AIPurchaseBehavior.EvaluatePurchase",
                $"Player: {player.GetPName(), -10} || Space: {ownableSpace.spaceName,-10} || WillBuy: {decision,-5}\n" +
                $"Score: {score,-5} || Threshold: {threshold,-5}",
                LogCategory.AI);

            return decision;
        }

        public float TestPurchaseScore(OwnableSpaceData ownableSpace)
        {
            return CalculatePurchaseScore(ownableSpace);
        }

        public float TestThreshold()
        {
            return CalculateThreshold();
        }

        private float CalculatePurchaseScore(OwnableSpaceData ownableSpace)
        {
            float score = purchaseWeights.baseValueScore;

            float reserveScore = EvaluateReserveCushion(ownableSpace);
            if (reserveScore <= -900f) // cant afford, early exit
                return reserveScore;
            score += reserveScore;

            score += EvaluateColorGroupProgress(ownableSpace);
            score += EvaluateMonopolyCompletion(ownableSpace);
            score += EvaluateHighValueProperty(ownableSpace);

            return score;
        }
        
        private float CalculateThreshold()
        {
            float wealthRatio = (float)player.GetMoney() / purchaseThresholds.startingCash;
            float wealthAdjustment = -purchaseThresholds.wealthThresholdReduction * wealthRatio;
            float randomness = Random.Range(-purchaseThresholds.randomVariance, purchaseThresholds.randomVariance);

            return purchaseThresholds.basePurchaseThreshold + wealthAdjustment + randomness;
        }

        private float EvaluateReserveCushion(OwnableSpaceData ownableSpace)
        {
            int afterPurchase = player.GetMoney() - ownableSpace.buyPrice;
            int minReserve = Mathf.RoundToInt(
                    purchaseThresholds.startingCash * purchaseThresholds.minimumReservePercent);

            if (afterPurchase < minReserve)
                return -999f; // cant afford, early exit

            float wealthRatio = (float)afterPurchase / purchaseThresholds.startingCash;
            return wealthRatio * purchaseWeights.reserveCushionScore;
        }
        
        private float EvaluateColorGroupProgress(OwnableSpaceData ownableSpace)
        {
            int ownedInGroup = GetOwnedInGroup(ownableSpace);
            
            return ownedInGroup * purchaseWeights.colorGroupProgressScore;
        }
        
        private float EvaluateMonopolyCompletion(OwnableSpaceData ownableSpace)
        {
            int ownedInGroup = GetOwnedInGroup(ownableSpace);
            int totalInGroup = ownableSpace.numberOfPropertiesInGroup;

            bool wouldComplete = (ownedInGroup + 1 == totalInGroup);
            return wouldComplete ? purchaseWeights.monopolyCompletionBonus : 0f;
        }

        private int GetOwnedInGroup(OwnableSpaceData ownableSpace)
        {
            int ownedInGroup;

            if (ownableSpace is PropertySpaceData property)
            {
                ownedInGroup = player.GetNumberOfPropertiesOwnedByColor(property.groupColor);
            } 
            else if (ownableSpace is InstrumentSpaceData)
            {
                ownedInGroup = player.GetNumberInstrumentsOwned();
            }
            else // it's a planet
            {
                ownedInGroup = player.GetNumberPlanetsOwned();
            }
            
            return ownedInGroup;
        }
        
        private float EvaluateHighValueProperty(OwnableSpaceData ownableSpace)
        {
            if (ownableSpace.buyPrice >= purchaseThresholds.highValuePropertyThreshold)
                return purchaseWeights.highValuePropertyBonus;
            return 0f;
        }
    }
}