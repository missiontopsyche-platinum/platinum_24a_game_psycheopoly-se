using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Purchase;
using Assets.Scripts.Managers.Rules;

namespace Tests.EditMode.PurchaseManagerTests
{
    public class StandardPurchaseStrategyTests
    {
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
        public void Unowned_AffordableStreet_OffersToPlayer_UsesPurchasePrice()
        {
            var strat = new StandardPurchaseStrategy();
            var rules = new Rules();
            var own   = new Own();
            var buyer = P("Buyer", money: 1_000);

            var tile = new TestTileRentInfo
            {
                Name          = "Test Street",
                Type          = TileType.Street,
                Group         = ColorGroup.Red,
                BaseRent      = 10,
                HouseCount    = 0,
                IsMortgaged   = false,
                RentByHouses  = new[] { 10, 50, 150, 450, 625, 750 },
                PurchasePrice = 120 //this should come from adapter in actual game
            };

            var decision = strat.GetPurchaseDecision(tile, buyer, own, rules);

            Assert.AreEqual(PurchaseFlow.OfferToPlayer, decision.Flow);
            Assert.IsTrue(decision.CanAfford);
            Assert.AreEqual(120, decision.Price,
                "Expected strategy to use PurchasePrice from tile (adapter data) rather than a rent-based fallback.");

            Object.DestroyImmediate(buyer);
        }

        [Test]
        public void TileOwnedByOtherPlayer_NoOffer()
        {
            var strat = new StandardPurchaseStrategy();
            var rules = new Rules();
            var own   = new Own();
            var buyer = P("Buyer", 1_000);
            var other = P("Other", 1_000);

            var tile = new TestTileRentInfo
            {
                Name          = "Owned Street",
                Type          = TileType.Street,
                Group         = ColorGroup.LightBlue,
                BaseRent      = 20,
                HouseCount    = 0,
                IsMortgaged   = false,
                RentByHouses  = new[] { 20, 100, 300, 750, 925, 1100 },
                PurchasePrice = 60
            };

            own.SetOwner(tile, other);

            var decision = strat.GetPurchaseDecision(tile, buyer, own, rules);

            Assert.AreEqual(PurchaseFlow.None, decision.Flow);
            Assert.IsFalse(decision.CanAfford);

            Object.DestroyImmediate(buyer);
            Object.DestroyImmediate(other);
        }

        [Test]
        public void BuyerCannotAfford_NoOffer()
        {
            var strat = new StandardPurchaseStrategy();
            var rules = new Rules();
            var own   = new Own();
            var buyer = P("Poor Buyer", money: 0);

            var tile = new TestTileRentInfo
            {
                Name          = "Expensive",
                Type          = TileType.Street,
                Group         = ColorGroup.Green,
                BaseRent      = 100,
                HouseCount    = 0,
                IsMortgaged   = false,
                RentByHouses  = new[] { 100, 500, 1500, 4500, 6250, 7500 },
                PurchasePrice = 400
            };

            var decision = strat.GetPurchaseDecision(tile, buyer, own, rules);

            Assert.AreEqual(PurchaseFlow.None, decision.Flow);
            Assert.IsFalse(decision.CanAfford);

            Object.DestroyImmediate(buyer);
        }
    }
}
