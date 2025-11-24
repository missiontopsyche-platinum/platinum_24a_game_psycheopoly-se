using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Purchase;

namespace Tests.EditMode.PurchaseTests
{
    public class StandardPurchaseStrategyTests
    {
        private class Tile : ITileRentInfo
        {
            public string Name        { get; set; }
            public TileType Type      { get; set; }
            public ColorGroup Group   { get; set; }
            public bool IsMortgaged   { get; set; }
            public int HouseCount     { get; set; }
            public int BaseRent       { get; set; }
            public int[] RentByHouses { get; set; }
        }

        private class Rules : IRuleSet
        {
            public int RailroadBaseRent()      => 25;
            public int UtilityRentSingleMult() => 4;
            public int UtilityRentBothMult()   => 10;

            public int StreetsInGroup(ColorGroup g) =>
                (g == ColorGroup.Brown || g == ColorGroup.DarkBlue) ? 2 : 3;
        }

        private class Own : IOwnershipService
        {
            private readonly System.Collections.Generic.Dictionary<ITileRentInfo, Player> map =
                new System.Collections.Generic.Dictionary<ITileRentInfo, Player>();

            public void SetOwner(ITileRentInfo t, Player p) => map[t] = p;
            public Player GetOwner(ITileRentInfo t) => map.TryGetValue(t, out var p) ? p : null;

            public int CountOwnedInGroup(Player owner, ColorGroup group) => 0;
            public int CountRailroadsOwned(Player owner) => 0;
            public bool OwnsBothUtilities(Player owner) => false;
        }

        private Player P(string name, int money)
        {
            var p = ScriptableObject.CreateInstance<Player>();
            p.SetPName(name);
            p.SetMoney(money);
            return p;
        }

        [Test]
        public void Unowned_AffordableStreet_OffersToPlayer()
        {
            var strat = new StandardPurchaseStrategy();
            var rules = new Rules();
            var own   = new Own();
            var buyer = P("Buyer", money: 1_000);

            var tile = new Tile
            {
                Name        = "Test Street",
                Type        = TileType.Street,
                Group       = ColorGroup.Red,
                BaseRent    = 10,
                HouseCount  = 0,
                IsMortgaged = false,
                RentByHouses = new[] { 10, 50, 150, 450, 625, 750 }
            };

            var decision = strat.GetPurchaseDecision(tile, buyer, own, rules);

            Assert.AreEqual(PurchaseFlow.OfferToPlayer, decision.Flow);
            Assert.IsTrue(decision.CanAfford);
            Assert.Greater(decision.Price, 0);
        }

        [Test]
        public void TileOwnedByOtherPlayer_NoOffer()
        {
            var strat = new StandardPurchaseStrategy();
            var rules = new Rules();
            var own   = new Own();
            var buyer = P("Buyer", 1_000);
            var other = P("Other", 1_000);

            var tile = new Tile
            {
                Name        = "Owned Street",
                Type        = TileType.Street,
                Group       = ColorGroup.LightBlue,
                BaseRent    = 20,
                HouseCount  = 0,
                IsMortgaged = false,
                RentByHouses = new[] { 20, 100, 300, 750, 925, 1100 }
            };

            own.SetOwner(tile, other);

            var decision = strat.GetPurchaseDecision(tile, buyer, own, rules);

            Assert.AreEqual(PurchaseFlow.None, decision.Flow);
            Assert.IsFalse(decision.CanAfford);
        }

        [Test]
        public void BuyerCannotAfford_NoOffer()
        {
            var strat = new StandardPurchaseStrategy();
            var rules = new Rules();
            var own   = new Own();
            var buyer = P("Poor Buyer", money: 0);

            var tile = new Tile
            {
                Name        = "Expensive",
                Type        = TileType.Street,
                Group       = ColorGroup.Green,
                BaseRent    = 100,
                HouseCount  = 0,
                IsMortgaged = false,
                RentByHouses = new[] { 100, 500, 1500, 4500, 6250, 7500 }
            };

            var decision = strat.GetPurchaseDecision(tile, buyer, own, rules);

            Assert.AreEqual(PurchaseFlow.None, decision.Flow);
            Assert.IsFalse(decision.CanAfford);
        }
    }
}
