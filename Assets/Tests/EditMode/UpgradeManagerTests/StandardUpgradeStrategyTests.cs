using Assets.Scripts.Managers.Rent;
using NUnit.Framework;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class StandardUpgradeStrategyTests : UpgradeManagerTestBase
    {
        private sealed class FakeUpgradableTile : IUpgradableTileInfo
        {
            public string Name => "Fake";
            public TileType Type { get; set; } = TileType.Street;
            public ColorGroup Group => ColorGroup.None;
            public bool IsMortgaged { get; set; }
            public int PurchasePrice => 0;

            public int UpgradeLevel { get; set; }
            public int MaxUpgradeLevel { get; set; } = 5;
            public bool IsMaxed { get; set; }

            public int NextCost { get; set; } = 50;

            public int UpgradeCost => throw new System.NotImplementedException();

            public int[] UpgradeCostByLevel => throw new System.NotImplementedException();

            public int HouseCount => throw new System.NotImplementedException();

            public int BaseRent => throw new System.NotImplementedException();

            public int[] RentByHouses => throw new System.NotImplementedException();

            public int GetNextUpgradeCost() => NextCost;

            public void ApplyUpgrade() => UpgradeLevel++;
        }

        [Test]
        public void GetUpgradeDecision_NotAllowed_WhenTileOrOwnerNull()
        {
            var strat = new StandardUpgradeStrategy();
            var owner = CreatePlayer(100);

            var d1 = strat.GetUpgradeDecision(null, owner);
            Assert.IsFalse(d1.Allowed);
            Assert.AreEqual(0, d1.Cost);

            var tile = new FakeUpgradableTile();
            var d2 = strat.GetUpgradeDecision(tile, null);
            Assert.IsFalse(d2.Allowed);
            Assert.AreEqual(0, d2.Cost);
        }

        [Test]
        public void GetUpgradeDecision_NotAllowed_WhenNotStreetOrMortgagedOrMaxed()
        {
            var strat = new StandardUpgradeStrategy();
            var owner = CreatePlayer(999);

            var notStreet = new FakeUpgradableTile { Type = TileType.Utility, NextCost = 50 };
            Assert.IsFalse(strat.GetUpgradeDecision(notStreet, owner).Allowed);

            var mortgaged = new FakeUpgradableTile { Type = TileType.Street, IsMortgaged = true, NextCost = 50 };
            Assert.IsFalse(strat.GetUpgradeDecision(mortgaged, owner).Allowed);

            var maxed = new FakeUpgradableTile { Type = TileType.Street, IsMaxed = true, NextCost = 50 };
            Assert.IsFalse(strat.GetUpgradeDecision(maxed, owner).Allowed);
        }

        [Test]
        public void GetUpgradeDecision_NotAllowed_WhenCostNonPositive()
        {
            var strat = new StandardUpgradeStrategy();
            var owner = CreatePlayer(999);

            var tile = new FakeUpgradableTile { Type = TileType.Street, NextCost = 0 };
            var d = strat.GetUpgradeDecision(tile, owner);

            Assert.IsFalse(d.Allowed);
            Assert.AreEqual(0, d.Cost);
        }

        [Test]
        public void GetUpgradeDecision_NotAllowed_WhenOwnerCannotAfford()
        {
            var strat = new StandardUpgradeStrategy();
            var owner = CreatePlayer(10);

            var tile = new FakeUpgradableTile { Type = TileType.Street, NextCost = 50 };
            var d = strat.GetUpgradeDecision(tile, owner);

            Assert.IsFalse(d.Allowed);
            Assert.AreEqual(50, d.Cost);
        }

        [Test]
        public void GetUpgradeDecision_Allowed_WhenAllChecksPass()
        {
            var strat = new StandardUpgradeStrategy();
            var owner = CreatePlayer(100);

            var tile = new FakeUpgradableTile { Type = TileType.Street, NextCost = 50, IsMortgaged = false, IsMaxed = false };
            var d = strat.GetUpgradeDecision(tile, owner);

            Assert.IsTrue(d.Allowed);
            Assert.AreEqual(50, d.Cost);
        }
    }
}
