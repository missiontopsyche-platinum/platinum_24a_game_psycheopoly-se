using NUnit.Framework;
using UnityEngine;
using Tests.EditMode;
using Assets.Scripts.Managers.Rent;

namespace Tests.EditMode.RentTests
{
    public class RentTestBase : ManagerTestBase
    {
        protected GameObject rentGO;
        protected RentManager rentManager;
        protected EconomyAdapter economy;
        protected OwnershipServiceAdapter ownership;
        protected RentModifierService rentModifiers;

        protected Player owner;
        protected Player tenant;

        [SetUp]
        public virtual void SetUp()
        {
            owner = ScriptableObject.CreateInstance<Player>();
            owner.SetPName("Owner");
            owner.SetMoney(10_000);

            tenant = ScriptableObject.CreateInstance<Player>();
            tenant.SetPName("Tenant");
            tenant.SetMoney(10_000);

            rentGO = new GameObject("RentManager");

            // Add dependencies first so we know exactly what exists.
            economy = rentGO.AddComponent<EconomyAdapter>();
            ownership = rentGO.AddComponent<OwnershipServiceAdapter>();
            rentModifiers = rentGO.AddComponent<RentModifierService>();

            // Add manager last.
            rentManager = rentGO.AddComponent<RentManager>();

            Assert.NotNull(rentManager);
            Assert.NotNull(economy);
            Assert.NotNull(ownership);
            Assert.NotNull(rentModifiers);
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(rentGO);
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tenant);
        }

        protected ITileRentInfo Street(
            string name,
            ColorGroup group,
            int baseRent,
            int houses = 0,
            bool mortgaged = false,
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
                RentByHouses = rentTable ?? new[]
                {
                    baseRent,
                    baseRent * 5,
                    baseRent * 15,
                    baseRent * 37,
                    baseRent * 46,
                    baseRent * 55
                }
            };
        }

        protected ITileRentInfo Railroad(string name = "RR")
        {
            return new FakeTile
            {
                Name = name,
                Type = TileType.Railroad,
                Group = ColorGroup.None,
                BaseRent = 0,
                HouseCount = 0,
                IsMortgaged = false,
                RentByHouses = new int[0]
            };
        }

        protected ITileRentInfo Utility(string name)
        {
            return new FakeTile
            {
                Name = name,
                Type = TileType.Utility,
                Group = ColorGroup.None,
                BaseRent = 0,
                HouseCount = 0,
                IsMortgaged = false,
                RentByHouses = new int[0]
            };
        }

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