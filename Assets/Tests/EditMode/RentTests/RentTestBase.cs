using NUnit.Framework;
using UnityEngine;
using Tests.EditMode; // adjust if your ManagerTestBase namespace differs
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;
namespace Tests.EditMode.RentTests
{
    public class RentTestBase : ManagerTestBase
    {
        protected GameObject rentGO;
        protected RentManager rentManager;
        protected EconomyAdapter economy;
        protected OwnershipServiceAdapter ownership;

        protected Player owner;
        protected Player tenant;

        protected StandardRuleSet rules;

        [SetUp]
        public virtual void SetUp()
        {
            owner  = ScriptableObject.CreateInstance<Player>();
            owner.SetPName("Owner");
            owner.SetMoney(10_000);

            tenant = ScriptableObject.CreateInstance<Player>();
            tenant.SetPName("Tenant");
            tenant.SetMoney(10_000);

            rentGO      = new GameObject("RentManager");
            rentManager = rentGO.AddComponent<RentManager>();

            economy     = rentGO.AddComponent<EconomyAdapter>();
            ownership   = rentGO.AddComponent<OwnershipServiceAdapter>();

            rules = StandardRuleSet.GetInstance();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(rentGO);
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tenant);
        }

        protected ITileRentInfo Street(
            string name, ColorGroup group, int baseRent, int houses = 0, bool mortgaged = false,
            int[] rentTable = null)
        {
            return new FakeTile
            {
                Name = name,
                Type = TileType.Street,
                Group = group,
                BaseRent = baseRent,
                HouseCount = houses,
                IsMortgaged = mortgaged,
                RentByHouses = rentTable ?? new[] { baseRent, baseRent * 5, baseRent * 15, baseRent * 37, baseRent * 46, baseRent * 55 }
            };
        }

        protected ITileRentInfo Railroad(string name = "RR") => new FakeTile
        {
            Name = name, Type = TileType.Railroad, Group = ColorGroup.None, BaseRent = 0,
            HouseCount = 0, IsMortgaged = false, RentByHouses = new int[0]
        };

        protected ITileRentInfo Utility(string name) => new FakeTile
        {
            Name = name, Type = TileType.Utility, Group = ColorGroup.None, BaseRent = 0,
            HouseCount = 0, IsMortgaged = false, RentByHouses = new int[0]
        };

        private class FakeTile : ITileRentInfo
        {
            public string Name { get; set; }
            public TileType Type { get; set; }
            public ColorGroup Group { get; set; }
            public bool IsMortgaged { get; set; }
            public int HouseCount { get; set; }
            public int BaseRent { get; set; }
            public int[] RentByHouses { get; set; }
        }
    }
}
