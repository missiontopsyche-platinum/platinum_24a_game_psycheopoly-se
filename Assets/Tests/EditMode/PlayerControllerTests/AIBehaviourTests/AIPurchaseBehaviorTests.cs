using AIBehavior;
using Data;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PlayerControllerTests.AIBehaviourTests
{
    public class AIPurchaseBehaviorTests: PlayerControllerTestBase
    {
        private PurchaseWeights weights;
        private PurchaseThresholds thresholds;

        [SetUp]
        public void SetUp()
        {
            weights = new PurchaseWeights();
            thresholds = new PurchaseThresholds() { randomVariance = 0 };
        }
        
        // Hard Veto Tests
        [Test]
        public void CashMinusPriceBelowMinimumReserve_ReturnsFalse()
        {
            // default min reserve is 15% of $1500, or $225
            // $300 - $200 = $100 remaining, which is below $225
            var player = Track(APlayer()
                .WithMoney(300)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(200)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(-999f, behavior.TestPurchaseScore(property), FLOAT_TOLERANCE);
        }

        [Test]
        public void CashMinusPriceEqualsMinimumReserve_ReturnsTrue()
        {
            // default min reserve is 15% of $1500, or $225
            // $425 - $200 = $225 remaining, which exactly equals minimum reserve
            
            // removes other scoring calculations to isolate the base purchase score
            weights.reserveCushionScore = 0;

            var player = Track(APlayer()
                .WithMoney(425)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(200)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(weights.baseValueScore, behavior.TestPurchaseScore(property), FLOAT_TOLERANCE);
        }
        
        // Reserve Cushion Tests
        [Test]
        public void HigherWealthRatio_ProducesHigherScore()
        {
            var property = Track(AnOwnableSpace()
                .WithBuyPrice(100)
                .BuildAsProperty());
            var richPlayer = Track(APlayer()
                .WithMoney(1500)
                .Build());
            var poorPlayer = Track(APlayer()
                .WithMoney(500)
                .Build());

            var richBehavior = new AIPurchaseBehavior(richPlayer, weights, thresholds);
            var poorBehavior = new AIPurchaseBehavior(poorPlayer, weights, thresholds);
            
            Assert.Greater(
                richBehavior.TestPurchaseScore(property), 
                poorBehavior.TestPurchaseScore(property));
        }

        [Test]
        public void AtFullStartingCash_ReserveCushionIsMaximum()
        {
            // $1500 - $60 = $1440 remaining
            // $1440 / $1500 = 0.96 wealth ratio
            // 0.96 * 20 (default cushion weight) + 10 (base) = 29.2
            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(60)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(29.2f, behavior.TestPurchaseScore(property), FLOAT_TOLERANCE);
        }
        
        // Wealth Adjusted Threshold Tests
        [Test]
        public void AtFullStartingCash_ThresholdIsSignificantlyReduced()
        {
            // At 100% wealth, reduction is -30 (default), so threshold = 50 - 30 = 20
            // random variance is turned off for tests.
            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(20f, behavior.TestThreshold(), FLOAT_TOLERANCE);
        }

        [Test]
        public void AtMinimumReserve_ThresholdIsNearBaseValue()
        {
            // At 15% wealth ($225), reduction is -30 * 0.15 = -4.5, so threshold = 50 - 4.5 = 45.5
            var player = Track(APlayer()
                .WithMoney(225)
                .Build());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(45.5f, behavior.TestThreshold(), FLOAT_TOLERANCE);
        }

        [Test]
        public void RandomnessRangeZero_ThresholdIsDeterministic()
        {
            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            float firstCall = behavior.TestThreshold();
            float secondCall = behavior.TestThreshold();

            Assert.AreEqual(firstCall, secondCall);
        }
        
        // Color Group Progress Tests
        [Test]
        public void PlayerOwnsNoneOfGroup_ReturnsZeroProgressScore()
        {
            weights.reserveCushionScore = 0;

            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(weights.baseValueScore, behavior.TestPurchaseScore(property), FLOAT_TOLERANCE);
        }

        [Test]
        public void PlayerOwnsOneOfGroup_ReturnsSingleScoreBonus()
        {
            weights.reserveCushionScore = 0;

            var owned = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(1500)
                .WithOwnedProperty(owned)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            float expected = weights.baseValueScore + weights.colorGroupProgressScore;
            Assert.AreEqual(expected, behavior.TestPurchaseScore(target), FLOAT_TOLERANCE);
        }

        [Test]
        public void PlayerOwnsTwoOfGroup_ReturnsDoubleScoreBonus()
        {
            weights.reserveCushionScore = 0;
            weights.monopolyCompletionBonus = 0;

            var owned1 = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var owned2 = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(1500)
                .WithOwnedProperties(owned1, owned2)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            float expected = weights.baseValueScore + (weights.colorGroupProgressScore * 2);
            Assert.AreEqual(expected, behavior.TestPurchaseScore(target), FLOAT_TOLERANCE);
        }
        
        // Monopoly Completion Tests
        [Test]
        public void CompletingTwoPropertyGroup_ReturnsMonopolyBonus()
        {
            weights.reserveCushionScore = 0;
            weights.colorGroupProgressScore = 0;

            var owned = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(1500)
                .WithOwnedProperty(owned)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            float expected = weights.baseValueScore + weights.monopolyCompletionBonus;
            Assert.AreEqual(expected, behavior.TestPurchaseScore(target), FLOAT_TOLERANCE);
        }

        [Test]
        public void CompletingThreePropertyGroup_ReturnsMonopolyBonus()
        {
            weights.reserveCushionScore = 0;
            weights.colorGroupProgressScore = 0;

            var owned1 = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var owned2 = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(1500)
                .WithOwnedProperties(owned1, owned2)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            float expected = weights.baseValueScore + weights.monopolyCompletionBonus;
            Assert.AreEqual(expected, behavior.TestPurchaseScore(target), FLOAT_TOLERANCE);
        }

        [Test]
        public void OneAwayFromCompletingGroup_ReturnsNoMonopolyBonus()
        {
            weights.reserveCushionScore = 0;
            weights.colorGroupProgressScore = 0;

            var owned = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(1500)
                .WithOwnedProperty(owned)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(weights.baseValueScore, behavior.TestPurchaseScore(target), FLOAT_TOLERANCE);
        }
        
        // High Value Property Tests
        [Test]
        public void AtExactlyHighValueThreshold_ReturnsBonus()
        {
            weights.reserveCushionScore = 0;

            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(thresholds.highValuePropertyThreshold)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            float expected = weights.baseValueScore + weights.highValuePropertyBonus;
            Assert.AreEqual(expected, behavior.TestPurchaseScore(property), FLOAT_TOLERANCE);
        }

        [Test]
        public void BelowHighValueThreshold_ReturnsNoBonus()
        {
            weights.reserveCushionScore = 0;

            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(thresholds.highValuePropertyThreshold - 1)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.AreEqual(weights.baseValueScore, behavior.TestPurchaseScore(property), FLOAT_TOLERANCE);
        }
        
        // Integration Tests
        [Test]
        public void EarlyGameFullCashNoColorProgress_ReturnsTrue()
        {
            var player = Track(APlayer()
                .WithMoney(1500)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(60)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.IsTrue(behavior.EvaluatePurchase(property));
        }

        [Test]
        public void LateGameLowCashNoColorProgress_ReturnsFalse()
        {
            var player = Track(APlayer()
                .WithMoney(300)
                .Build());

            var property = Track(AnOwnableSpace()
                .WithBuyPrice(60)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.IsFalse(behavior.EvaluatePurchase(property));
        }

        [Test]
        public void LateGameLowCashCompletingMonopoly_ReturnsTrue()
        {
            var owned1 = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var owned2 = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(300)
                .WithOwnedProperties(owned1, owned2)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .WithBuyPrice(60)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.IsTrue(behavior.EvaluatePurchase(target));
        }

        [Test]
        public void MidGameModerateCashWithColorProgress_ReturnsTrue()
        {
            var owned = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty());

            var player = Track(APlayer()
                .WithMoney(750)
                .WithOwnedProperty(owned)
                .Build());

            var target = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .WithBuyPrice(100)
                .BuildAsProperty());

            var behavior = new AIPurchaseBehavior(player, weights, thresholds);

            Assert.IsTrue(behavior.EvaluatePurchase(target));
        }
    }
}