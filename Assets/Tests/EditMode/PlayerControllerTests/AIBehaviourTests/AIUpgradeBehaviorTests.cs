using AIBehavior;
using Data;
using NUnit.Framework;
using Tests.EditMode.PlayerControllerTests.Builders;
using UnityEngine;

namespace Tests.EditMode.PlayerControllerTests.AIBehaviourTests
{
    public class AIUpgradeBehaviorTests: PlayerControllerTestBase
    {
        private UpgradeWeights weights;
        private UpgradeThresholds thresholds;

        [SetUp]
        public void SetUp()
        {
            weights = new UpgradeWeights();
            thresholds = new UpgradeThresholds() { randomVariance = 0 };
        }

        [Test] 
        public void BaseUpgradeScore_Level0_AppliesCorrectMultiplier()
        {
            weights.roiWeight = 0;
            weights.reserveCushionWeight = 0;
            
            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithResearchFundingValues(new[] { 10, 50, 100, 200, 300, 400 })
                .WithDataPointCost(50)
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithOwnedProperty(property)
                .Build());

            var behavior = new AIUpgradeBehavior(player, weights, thresholds);
            float score = behavior.TestCalculateScore(property);
            
            float expectedScore = weights.baseUpgradeScore * weights.upgradeLevelWeight[0];
            Assert.AreEqual(expectedScore, score, FLOAT_TOLERANCE);
        }

        [Test]
        public void BaseUpgradeScore_Level3_AppliesCorrectMultipler()
        {
            weights.roiWeight = 0;
            weights.reserveCushionWeight = 0;
            
            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(3)
                .WithResearchFundingValues(new[] { 10, 50, 100, 200, 300, 400 })
                .WithDataPointCost(100)
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithOwnedProperty(property)
                .Build());

            var behavior = new AIUpgradeBehavior(player, weights, thresholds);
            float score = behavior.TestCalculateScore(property);
            
            float expectedScore = weights.baseUpgradeScore * weights.upgradeLevelWeight[3];
            Assert.AreEqual(expectedScore, score, FLOAT_TOLERANCE);
        }

        [Test]
        public void ROIScore_CalculatesNormalizedROICorrectly()
        {
            weights.baseUpgradeScore = 0;
            weights.reserveCushionWeight = 0;

            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithResearchFundingValues(new[] { 14, 70, 200, 550, 750, 950 })
                .WithDataPointCost(100)
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithOwnedProperty(property)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            float score = behavior.TestCalculateScore(property);
            
            // Rent delta at level 0->1: 70 - 14 = 56
            // Max delta: 350
            // Normalized ROI: 56 / 350 = 0.16
            float expectedScore = (56f / 350f) * weights.roiWeight;
            Assert.AreEqual(expectedScore, score, FLOAT_TOLERANCE);
        }

        [Test]
        public void ReserveCushion_CalculatesSigmoidCorrectly()
        {
            weights.baseUpgradeScore = 0;
            weights.roiWeight = 0;
            thresholds.startingCash = 1500;
            
            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithResearchFundingValues(new[] {10, 50, 100, 200, 300, 400})
                .WithDataPointCost(100)
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithMoney(1200)
                .WithOwnedProperty(property)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            float score = behavior.TestCalculateScore(property);
            
            // Reserve ratio: (1200 - 100) / 1500 = 0.733
            // Sigmoid(0.733) normalized ~= 0.917
            // (checked in Desmos: https://www.desmos.com/calculator/bjx7iwpa9e)
            float expectedScore = 0.917f * weights.reserveCushionWeight;
            Assert.AreEqual(expectedScore, score, FLOAT_TOLERANCE);
        }

        [Test]
        public void HardVeto_CashBelowMinimumReserve_DefersUpgrade()
        {
            thresholds.minimumReserve = 300;
            thresholds.baseThreshold = 0; // makes it so the score will certainly pass
            thresholds.randomVariance = 0;

            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithGroupSize(1)
                .WithGroupColor(Color.red)
                .WithDataPointCost(100)
                .WithResearchFundingValues(new[] { 10, 50, 100, 200, 300, 400 })
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithMoney(399)
                .WithOwnedProperty(property)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            var evaluation = behavior.EvaluateUpgrade();
            Assert.IsFalse(evaluation.willUpgrade);
        }

        [Test]
        public void ScoreBelowThreshold_DefersUpgrade()
        {
            // set this up to produce a low score (5 * 1.2 = 6)
            // that could never pass the threshold of 20
            weights.baseUpgradeScore = 5;
            weights.roiWeight = 0;
            weights.reserveCushionWeight = 0;
            thresholds.baseThreshold = 20;

            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithGroupSize(1)
                .WithGroupColor(Color.red)
                .WithDataPointCost(100)
                .WithResearchFundingValues(new[] { 10, 50, 100, 200, 300, 400 })
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithMoney(1000)
                .WithOwnedProperty(property)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            var evaluation = behavior.EvaluateUpgrade();
            Assert.IsFalse(evaluation.willUpgrade);
        }

        [Test]
        public void ScoreAboveThreshold_TriggersUpgrade()
        {
            // set up to produce a score that is by default
            // higher than the threshold
            weights.baseUpgradeScore = 50;
            weights.roiWeight = 0;
            weights.reserveCushionWeight = 0;
            thresholds.baseThreshold = 20;
            
            var property = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithGroupSize(1)
                .WithGroupColor(Color.red)
                .WithDataPointCost(100)
                .WithResearchFundingValues(new[] { 10, 50, 100, 200, 300, 400 })
                .BuildAsProperty());
            var player = Track(APlayer()
                .WithMoney(1000)
                .WithOwnedProperty(property)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            var evaluation = behavior.EvaluateUpgrade();
            Assert.IsTrue(evaluation.willUpgrade);
            Assert.AreEqual(property, evaluation.upgradeTarget);
        }

        [Test]
        public void MultipleProperties_PicksHighestScoring()
        {
            // lower the threshold to allow both properties to pass
            thresholds.baseThreshold = 20;
    
            // Property with lower ROI (scores ~28)
            var lowROI = Track(AnOwnableSpace()
                .WithUpgradeLevel(0)
                .WithGroupSize(1)
                .WithGroupColor(Color.red)
                .WithDataPointCost(100)
                .WithResearchFundingValues(new[] {10, 50, 100, 200, 300, 400})
                .BuildAsProperty());
    
            // Property with higher ROI (scores ~54)
            var highROI = Track(AnOwnableSpace()
                .WithUpgradeLevel(2)
                .WithGroupSize(1)
                .WithGroupColor(Color.blue)
                .WithDataPointCost(100)
                .WithResearchFundingValues(new[] {14, 70, 200, 550, 750, 950})
                .BuildAsProperty());
    
            var player = Track(APlayer()
                .WithMoney(1000)
                .WithOwnedProperty(lowROI)
                .WithOwnedProperty(highROI)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            var evaluation = behavior.EvaluateUpgrade();
            Assert.IsTrue(evaluation.willUpgrade);
            Assert.AreEqual(highROI, evaluation.upgradeTarget);
        }

        [Test]
        public void EmptyEligibleList_DefersUpgrade()
        {
            var player = Track(APlayer()
                .WithMoney(1000)
                .Build());
            var behavior = new AIUpgradeBehavior(player, weights, thresholds);

            var evaluation = behavior.EvaluateUpgrade();
            Assert.IsFalse(evaluation.willUpgrade);
        }
    }
}