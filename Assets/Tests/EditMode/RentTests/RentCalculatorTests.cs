using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;

namespace Tests.EditMode.RentTests
{
    public class RentCalculatorTests
    {
        private class Tile : ITileRentInfo
        {
            public string Name { get; set; }
            public TileType Type { get; set; }
            public ColorGroup Group { get; set; }
            public bool IsMortgaged { get; set; }
            public int HouseCount { get; set; }
            public int BaseRent { get; set; }
            public int[] RentByHouses { get; set; }
        }

        private class Rules : IRuleSet
        {
            public int RailroadBaseRent() => 25;
            public int UtilityRentSingleMult() => 4;
            public int UtilityRentBothMult() => 10;
            public int StreetsInGroup(ColorGroup g) =>
                (g == ColorGroup.Brown || g == ColorGroup.DarkBlue) ? 2 : 3;
            public int PlayerStartingMoney() => 1500;
            public int GOSalary() => 200;
            public int JailFee() => 50;
            public WinConditionType WinCondition() => WinConditionType.LastPlayerStanding;
            public int TargetMoney() => 5000;
            public int TurnLimit() => 20;
            public int MaxJailTurns() => 3;
        }

        private class Own : IOwnershipService
        {
            private readonly System.Collections.Generic.Dictionary<ITileRentInfo, Player> map =
                new System.Collections.Generic.Dictionary<ITileRentInfo, Player>();

            public void SetOwner(ITileRentInfo t, Player p) => map[t] = p;
            public Player GetOwner(ITileRentInfo t) => map.TryGetValue(t, out var p) ? p : null;

            public int CountOwnedInGroup(Player owner, ColorGroup group)
            {
                int c = 0;
                foreach (var kv in map)
                    if (kv.Value == owner && kv.Key.Type == TileType.Street && kv.Key.Group == group) c++;
                return c;
            }

            public int CountRailroadsOwned(Player owner)
            {
                int c = 0;
                foreach (var kv in map)
                    if (kv.Value == owner && kv.Key.Type == TileType.Railroad) c++;
                return c;
            }

            public bool OwnsBothUtilities(Player owner)
            {
                int c = 0;
                foreach (var kv in map)
                    if (kv.Value == owner && kv.Key.Type == TileType.Utility) c++;
                return c >= 2;
            }
        }

        private Player P(string name)
        {
            var p = ScriptableObject.CreateInstance<Player>();
            p.SetPName(name);
            p.SetMoney(9999);
            return p;
        }

        [Test]
        public void Street_NoHouses_NoMonopoly_Base()
        {
            var rules = new Rules();
            var own = new Own();
            var o = P("O");

            var t = new Tile {
                Name="Mediterranean", Type=TileType.Street, Group=ColorGroup.Brown,
                BaseRent=2, HouseCount=0, RentByHouses=new[]{2,10,30,90,160,250}
            };

            own.SetOwner(t, o);
            Assert.AreEqual(2, RentCalculator.ComputeRent(t, o, 0, own, rules));
            Object.DestroyImmediate(o);
        }

        [Test]
        public void Street_NoHouses_WithMonopoly_DoubleBase()
        {
            var rules = new Rules();
            var own = new Own();
            var o = P("O");

            var t1 = new Tile { Name="Mediterranean", Type=TileType.Street, Group=ColorGroup.Brown, BaseRent=2, HouseCount=0, RentByHouses=new[]{2,10,30,90,160,250} };
            var t2 = new Tile { Name="Baltic",        Type=TileType.Street, Group=ColorGroup.Brown, BaseRent=4, HouseCount=0, RentByHouses=new[]{4,20,60,180,320,450} };

            own.SetOwner(t1, o);
            own.SetOwner(t2, o);

            Assert.AreEqual(4, RentCalculator.ComputeRent(t1, o, 0, own, rules));
            Object.DestroyImmediate(o);
        }

        [Test]
        public void Street_WithHouses_UsesTable()
        {
            var rules = new Rules();
            var own = new Own();
            var o = P("O");

            var t = new Tile {
                Name="Illinois", Type=TileType.Street, Group=ColorGroup.Red, BaseRent=20,
                HouseCount=3, RentByHouses=new[]{20,100,300,750,925,1100}
            };
            own.SetOwner(t, o);

            Assert.AreEqual(750, RentCalculator.ComputeRent(t, o, 0, own, rules));
            Object.DestroyImmediate(o);
        }

        [Test]
        public void Railroad_Scales_25_50_100_200()
        {
            var rules = new Rules();
            var own = new Own();
            var o = P("O");

            var r1 = new Tile { Name="RR1", Type=TileType.Railroad };
            var r2 = new Tile { Name="RR2", Type=TileType.Railroad };
            var r3 = new Tile { Name="RR3", Type=TileType.Railroad };
            var r4 = new Tile { Name="RR4", Type=TileType.Railroad };

            own.SetOwner(r1, o);
            Assert.AreEqual(25, RentCalculator.ComputeRent(r1, o, 0, own, rules));
            own.SetOwner(r2, o);
            Assert.AreEqual(50, RentCalculator.ComputeRent(r1, o, 0, own, rules));
            own.SetOwner(r3, o);
            Assert.AreEqual(100, RentCalculator.ComputeRent(r1, o, 0, own, rules));
            own.SetOwner(r4, o);
            Assert.AreEqual(200, RentCalculator.ComputeRent(r1, o, 0, own, rules));

            Object.DestroyImmediate(o);
        }

        [Test]
        public void Utility_UsesDice_Multipliers_4_and_10()
        {
            var rules = new Rules();
            var own = new Own();
            var o = P("O");

            var u1 = new Tile { Name="Electric", Type=TileType.Utility };
            var u2 = new Tile { Name="Water",    Type=TileType.Utility };

            own.SetOwner(u1, o);
            Assert.AreEqual(28, RentCalculator.ComputeRent(u1, o, 7, own, rules)); // 7*4

            own.SetOwner(u2, o);
            Assert.AreEqual(70, RentCalculator.ComputeRent(u1, o, 7, own, rules)); // 7*10

            Object.DestroyImmediate(o);
        }

        [Test]
        public void Mortgaged_ReturnsZero()
        {
            var rules = new Rules();
            var own = new Own();
            var o = P("O");

            var t = new Tile {
                Name="Park Place", Type=TileType.Street, Group=ColorGroup.DarkBlue,
                BaseRent=35, HouseCount=0, IsMortgaged=true,
                RentByHouses=new[]{35,175,500,1100,1300,1500}
            };
            own.SetOwner(t, o);

            Assert.AreEqual(0, RentCalculator.ComputeRent(t, o, 0, own, rules));
            Object.DestroyImmediate(o);
        }
    }
}
