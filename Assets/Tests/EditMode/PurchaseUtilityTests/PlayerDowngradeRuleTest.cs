using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PurchaseUtilityTests
{
    public class PlayerDowngradeRuleTests
    {
        private Player player;

        [SetUp]
        public void SetUp()
        {
            player = ScriptableObject.CreateInstance<Player>();
            player.SetPName("Test Player");
            player.SetMoney(1500);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void FullMonopolyAllSameLevel()
        {
            var a = CreateProperty(Color.red, 3, 1, false);
            var b = CreateProperty(Color.red, 3, 1, false);
            var c = CreateProperty(Color.red, 3, 1, false);

            AddOwned(player, a, b, c);

            List<PropertySpaceData> result = player.GetValidDowngradableProperties();

            Assert.AreEqual(3, result.Count);
            Assert.Contains(a, result);
            Assert.Contains(b, result);
            Assert.Contains(c, result);

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void OnlyHighestLevelPropertyCanDowngrade()
        {
            var a = CreateProperty(Color.blue, 3, 3, false);
            var b = CreateProperty(Color.blue, 3, 2, false);
            var c = CreateProperty(Color.blue, 3, 2, false);

            AddOwned(player, a, b, c);

            List<PropertySpaceData> result = player.GetValidDowngradableProperties();

            Assert.AreEqual(1, result.Count);
            Assert.Contains(a, result);
            Assert.IsFalse(result.Contains(b));
            Assert.IsFalse(result.Contains(c));

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void DiscoveryLevelIsSellable()
        {
            var a = CreateProperty(Color.green, 3, 5, false);
            var b = CreateProperty(Color.green, 3, 4, false);
            var c = CreateProperty(Color.green, 3, 4, false);

            AddOwned(player, a, b, c);

            List<PropertySpaceData> result = player.GetValidDowngradableProperties();

            Assert.AreEqual(1, result.Count);
            Assert.Contains(a, result);

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void NotFullMonopoly()
        {
            var a = CreateProperty(Color.yellow, 3, 2, false);
            var b = CreateProperty(Color.yellow, 3, 2, false);

            AddOwned(player, a, b);

            List<PropertySpaceData> result = player.GetValidDowngradableProperties();

            Assert.AreEqual(0, result.Count);

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void MortgagedHighestProperty()
        {
            var a = CreateProperty(Color.magenta, 3, 3, true);
            var b = CreateProperty(Color.magenta, 3, 2, false);
            var c = CreateProperty(Color.magenta, 3, 2, false);

            AddOwned(player, a, b, c);

            List<PropertySpaceData> result = player.GetValidDowngradableProperties();

            Assert.AreEqual(0, result.Count);

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(c);
        }

        private static void AddOwned(Player owner, params PropertySpaceData[] properties)
        {
            foreach (var property in properties)
            {
                property.SetOwner(owner);
                owner.AddOwnedProperty(property);
            }
        }

        private static PropertySpaceData CreateProperty(Color groupColor, int propertiesInGroup, int level, bool mortgaged)
        {
            var property = ScriptableObject.CreateInstance<PropertySpaceData>();

            property.groupColor = groupColor;
            property.numberOfPropertiesInGroup = propertiesInGroup;
            property.SetResearchFundingValues(new[] { 10, 20, 30, 40, 50, 60 });
            property.SetDataPointCost(100);
            property.SetUpgradeLevel(level);

            property.isMortgaged = mortgaged;
            property.isMortgageable = !mortgaged;

            return property;
        }
    }
}