using System.Linq;
using NUnit.Framework;
using Tests.EditMode.PlayerControllerTests.Builders;
using UnityEngine;

namespace Tests.EditMode.PlayerTests
{
    public class PlayerGetValidUpgradeTargetsTests
    {
        private Player player;

        [SetUp]
        public void SetUp()
        {
            player = new MockPlayerBuilder().Build();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var property in player.GetOwnedProperties())
                Object.DestroyImmediate(property);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void NoOwnedProperties_ReturnsEmptyList()
        {
            var result = player.GetValidUpgradableProperties();
            Assert.IsEmpty(result);
        }

        [Test]
        public void PartialGroup_NoMonopoly_ReturnsEmptyList()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());

            var result = player.GetValidUpgradableProperties();
            Assert.IsEmpty(result);
        }

        [Test]
        public void FullMonopoly_AllAtLevelZero_ReturnsAllProperties()
        {
            var prop1 = new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty();
            var prop2 = new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty();

            Debug.Log($"prop1 groupSize: {prop1.numberOfPropertiesInGroup}");
            Debug.Log($"prop2 groupSize: {prop2.numberOfPropertiesInGroup}");

            player.AddOwnedProperty(prop1);
            player.AddOwnedProperty(prop2);

            var result = player.GetValidUpgradableProperties();
            Assert.AreEqual(2, result.Count);
            
            // player.AddOwnedProperty(new MockOwnableBuilder()
            //     .WithGroupColor(Color.red)
            //     .WithGroupSize(2)
            //     .BuildAsProperty());
            // player.AddOwnedProperty(new MockOwnableBuilder()
            //     .WithGroupColor(Color.red)
            //     .WithGroupSize(2)
            //     .BuildAsProperty());
            //
            // var result = player.GetValidUpgradableProperties();
            // Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void FullMonopoly_MixedLevels_ReturnsOnlyMinimumLevel()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .WithUpgradeLevel(1)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .WithUpgradeLevel(0)
                .BuildAsProperty());

            var result = player.GetValidUpgradableProperties();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].GetCurrentUpgradeLevel());
        }

        [Test]
        public void FullMonopoly_AllMaxed_ReturnsEmptyList()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .WithUpgradeLevel(5)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .WithUpgradeLevel(5)
                .BuildAsProperty());

            var result = player.GetValidUpgradableProperties();
            Assert.IsEmpty(result);
        }

        [Test]
        public void TwoMonopolies_BothAtZeroLevel_ReturnsAllProperties()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.blue)
                .WithGroupSize(2)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.blue)
                .WithGroupSize(2)
                .BuildAsProperty());

            var result = player.GetValidUpgradableProperties();
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void TwoMonopolies_OneMaxed_ReturnsOnlyNonMaxedGroup()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.blue)
                .WithGroupSize(2)
                .WithUpgradeLevel(5)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.blue)
                .WithGroupSize(2)
                .WithUpgradeLevel(5)
                .BuildAsProperty());

            var result = player.GetValidUpgradableProperties();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.groupColor == Color.red));
        }

        [Test]
        public void TwoMonopolies_DifferentLevels_ReachGroupReturnsItsMinimum()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .WithUpgradeLevel(1)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .WithUpgradeLevel(0)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.blue)
                .WithGroupSize(2)
                .WithUpgradeLevel(2)
                .BuildAsProperty());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.blue)
                .WithGroupSize(2)
                .WithUpgradeLevel(1)
                .BuildAsProperty());

            var result = player.GetValidUpgradableProperties();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result.Count(p => p.groupColor == Color.red));
            Assert.AreEqual(1, result.Count(p => p.groupColor == Color.blue));
        }

        [Test]
        public void OwnedNonPropertyTypes_AreIgnored()
        {
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsPlanet());
            player.AddOwnedProperty(new MockOwnableBuilder()
                .WithGroupColor(Color.red)
                .WithGroupSize(2)
                .BuildAsPlanet());

            var result = player.GetValidUpgradableProperties();
            Assert.IsEmpty(result);
        }
    }
}