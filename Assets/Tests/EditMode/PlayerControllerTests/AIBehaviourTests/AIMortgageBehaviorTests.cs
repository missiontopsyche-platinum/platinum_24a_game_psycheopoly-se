using UnityEngine;
using AIBehavior;
using Data;
using NUnit.Framework;

namespace Tests.EditMode.PlayerControllerTests.AIBehaviourTests
{
    public class AIMortgageBehaviorTests: PlayerControllerTestBase
    {
        private MortgageThresholds thresholds;

        [SetUp]
        public void SetUp()
        {
            thresholds = new MortgageThresholds();
        }

        [Test]
        public void EvaluateMortgage_AboveDangerThreshold_ReturnsEmptyQueue()
        {
            var player = Track(APlayer()
                .WithMoney(500)
                .Build()
            );

            var behavior = new AIMortgageBehavior(player, thresholds);
            var result = behavior.EvaluateMortgage();
            
            Assert.AreEqual(0, result.actions.Count);
        }

        [Test]
        public void BuildCandidates_InstrumentAndPlanet_GoToMortgagePool()
        {
            var instrument = Track(AnOwnableSpace().BuildAsInstrument());
            var planet = Track(AnOwnableSpace().BuildAsPlanet());

            var player = Track(APlayer()
                .WithMoney(0)
                .WithOwnedProperties(instrument, planet)
                .Build()
            );

            var behavior = new AIMortgageBehavior(player, thresholds);
            var (mortgagePool, sellPool) = behavior.TestBuildCandidatePools(
                player.GetUnmortgagedProperties());
            
            Assert.AreEqual(2, mortgagePool.Count);
            Assert.AreEqual(0, sellPool.Count);
        }

        [Test]
        public void BuildCandidatePools_IncompleteColorGroup_GoesToMortgagePool()
        {
            var property = Track(AnOwnableSpace()
                .WithGroupColor(Color.red)
                .WithGroupSize(3)
                .BuildAsProperty()
            );
            var player = Track(APlayer()
                .WithMoney(0)
                .WithOwnedProperty(property)
                .Build()
            );
            var behavior = new AIMortgageBehavior(player, thresholds);
            var (mortgagePool, sellPool) = behavior.TestBuildCandidatePools(
                player.GetUnmortgagedProperties());
            
            Assert.AreEqual(1, mortgagePool.Count);
            Assert.AreEqual(0, sellPool.Count);
        }

        [Test]
        public void BuildCandidatePools_CompleteGroupNoUpgrades_GoesToMortgagePool()
        {
            var color = Color.red;
            var prop1 = Track(AnOwnableSpace()
                .WithGroupColor(color)
                .WithGroupSize(2)
                .BuildAsProperty()
            );
            var prop2 = Track(AnOwnableSpace()
                .WithGroupColor(color)
                .WithGroupSize(2)
                .BuildAsProperty()
            );
            var player = Track(APlayer()
                .WithMoney(0)
                .WithOwnedProperties(prop1, prop2)
                .Build()
            );
            var behavior = new AIMortgageBehavior(player, thresholds);
            var (mortgagePool, sellPool) = behavior.TestBuildCandidatePools(
                player.GetUnmortgagedProperties());
            
            Assert.AreEqual(2, mortgagePool.Count);
            Assert.AreEqual(0, sellPool.Count);
        }
        
        [Test]
        public void BuildCandidatePools_CompleteGroupWithUpgrades_GoesToSellPool()
        {
            var color = Color.red;
            var prop1 = Track(AnOwnableSpace()
                .WithGroupColor(color)
                .WithGroupSize(2)
                .WithUpgradeLevel(2)
                .BuildAsProperty()
            );
            var prop2 = Track(AnOwnableSpace()
                .WithGroupColor(color)
                .WithGroupSize(2)
                .WithUpgradeLevel(1)
                .BuildAsProperty()
            );
            var player = Track(APlayer()
                .WithMoney(0)
                .WithOwnedProperties(prop1, prop2)
                .Build()
            );
            var behavior = new AIMortgageBehavior(player, thresholds);
            var (mortgagePool, sellPool) = behavior.TestBuildCandidatePools(
                player.GetUnmortgagedProperties());
            
            Assert.AreEqual(0, mortgagePool.Count);
            Assert.AreEqual(2, sellPool.Count);
        }
        
        [Test]
        public void CalculateScore_UnupgradedProperty_ReturnsCorrectRatio()
        {
            var property = Track(AnOwnableSpace()
                .WithCollabValue(80)
                .WithResearchFundingValues(new [] { 12, 60, 180, 500, 700, 900 })
                .BuildAsProperty()
            );

            var behavior = new AIMortgageBehavior(Track(APlayer().Build()), thresholds);
            var result = behavior.TestCalculateScore(property);

            // cashRaised: 80, recoveryCost: 88 (80 * 1.1), currentIncome: 12
            // 80 / (88 + 12) = 0.800
            Assert.AreEqual(0.800f, result, 0.001f);
        }

        [Test]
        public void CalculateScore_UpgradedProperty_ReturnsCorrectRatio()
        {
            var property = Track(AnOwnableSpace()
                .WithCollabValue(80)
                .WithResearchFundingValues(new [] { 12, 60, 180, 500, 700, 900 })
                .WithUpgradeLevel(2)
                .WithDataPointCost(50)
                .BuildAsProperty()
            );
            
            var behavior = new AIMortgageBehavior(Track(APlayer().Build()), thresholds);
            var result = behavior.TestCalculateScore(property);

            // cashRaised: 25, recoveryCost: 50 (sell datapoint cost), currentIncome: 120 (180-60)
            // 25 / (50 + 120) = 0.147
            Assert.AreEqual(0.147f, result, 0.001f);
        }
        
        [Test]
        public void CalculateScore_DiscoveryProperty_ReturnsCorrectRatio()
        {
            var property = Track(AnOwnableSpace()
                .WithCollabValue(80)
                .WithDataPointCost(50)
                .WithUpgradeLevel(5)
                .WithResearchFundingValues(new int[] { 12, 60, 180, 500, 700, 900 })
                .BuildAsProperty()
            );

            var behavior = new AIMortgageBehavior(Track(APlayer().Build()), thresholds);
            var result = behavior.TestCalculateScore(property);
            
            // cashRaised: 125 (50 * 5 / 2), recoveryCost: 250 (50 * 5), currentIncome: 888 (900 - 12)
            // 125 / (250 + 888) = 0.110
            Assert.AreEqual(0.110f, result, 0.001f);
        }
        
        [Test]
        public void CalculateScore_Instrument_ReturnsCorrectRatio()
        {
            var instrument = Track(AnOwnableSpace()
                .WithCollabValue(75)
                .WithResearchFundingValues(new [] {50, 100, 150, 200})
                .BuildAsInstrument()
            );

            var player = Track(APlayer()
                .WithOwnedProperty(instrument)
                .Build()
            );

            var behavior = new AIMortgageBehavior(player, thresholds);
            var result = behavior.TestCalculateScore(instrument);

            Debug.Log($"CalculateScore Instrument result: {result}");
            // cashRaised: 75, recoveryCost: 82.5 (75 * 1.1), currentIncome: 50
            // 75 / (82.5 + 50) = 0.556
            Assert.AreEqual(0.566f, result, 0.001f);
        }
        
        [Test]
        public void CalculateScore_Planet_ReturnsCorrectRatio()
        {
            var planet = Track(AnOwnableSpace()
                .WithCollabValue(100)
                .WithDiceMultipliers(new [] {4, 10})
                .BuildAsPlanet()
            );

            var player = Track(APlayer()
                .WithOwnedProperty(planet)
                .Build()
            );

            var behavior = new AIMortgageBehavior(player, thresholds);
            var result = behavior.TestCalculateScore(planet);

            Debug.Log($"CalculateScore Planet result: {result}");
            // cashRaised: 100, recoveryCost: 110 (100 * 1.1), currentIncome: 7 * 4
            // 100 / (110 + 28) = 0.725
            Assert.AreEqual(0.725f, result, 0.001f);
        }
        
        [Test]
        public void EvaluateMortgage_SellUpgradePromotesGroupToMortgagePool_CorrectActionSequence()
        {
            var color = Color.red;
            var upgradedProp = Track(AnOwnableSpace()
                .WithGroupColor(color)
                .WithGroupSize(2)
                .WithUpgradeLevel(1)
                .WithDataPointCost(50)
                .WithCollabValue(80)
                .WithResearchFundingValues(new [] { 12, 60, 180, 500, 700, 900 })
                .BuildAsProperty()
            );
            var unupgradedProp = Track(AnOwnableSpace()
                .WithGroupColor(color)
                .WithGroupSize(2)
                .WithCollabValue(60)
                .WithResearchFundingValues(new [] { 12, 60, 180, 500, 700, 900 })
                .BuildAsProperty()
            );

            var player = Track(APlayer()
                .WithMoney(0)
                .WithOwnedProperties(upgradedProp, unupgradedProp)
                .Build()
            );

            // set threshold really high to ensure we exhaust all choices
            thresholds.dangerThreshold = 1000; 
            var behavior = new AIMortgageBehavior(player, thresholds);
            var result = behavior.EvaluateMortgage();
            
            Assert.AreEqual(3, result.actions.Count);
            Assert.AreEqual(MortgageActionType.SellDataPoint, result.actions.Dequeue().actionType);
            Assert.AreEqual(MortgageActionType.Mortgage, result.actions.Dequeue().actionType);
            Assert.AreEqual(MortgageActionType.Mortgage, result.actions.Dequeue().actionType);
        }
    }
}