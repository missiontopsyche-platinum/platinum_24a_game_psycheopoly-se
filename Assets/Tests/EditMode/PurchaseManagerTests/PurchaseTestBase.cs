using NUnit.Framework;
using UnityEngine;
using Tests.EditMode;                 
using Assets.Scripts.Managers.Rent;   
using Assets.Scripts.Managers.Purchase;

namespace Tests.EditMode.PurchaseTests
{
    //Base class for purchase tests
    public class PurchaseTestBase : ManagerTestBase
    {
        protected GameObject purchaseGO;
        protected PurchaseManager purchaseManager;
        protected OwnershipServiceAdapter ownership;

        protected Player buyer;
        protected Player other;

        [SetUp]
        public virtual void SetUp()
        {
            //Players as ScriptableObjects
            buyer = ScriptableObject.CreateInstance<Player>();
            buyer.SetPName("Buyer");
            buyer.SetMoney(10_000);

            other = ScriptableObject.CreateInstance<Player>();
            other.SetPName("Other");
            other.SetMoney(10_000);

            //Manager and adapters
            purchaseGO = new GameObject("PurchaseManager");
            purchaseManager = purchaseGO.AddComponent<PurchaseManager>();
            ownership       = purchaseGO.AddComponent<OwnershipServiceAdapter>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(purchaseGO);
            Object.DestroyImmediate(buyer);
            Object.DestroyImmediate(other);
        }

        //Creates a fake ownable street tile for purchase tests
        protected ITileRentInfo Street(
            string name,
            ColorGroup group,
            int baseRent,
            bool mortgaged = false)
        {
            return new FakeTile
            {
                Name        = name,
                Type        = TileType.Street,
                Group       = group,
                BaseRent    = baseRent,
                HouseCount  = 0,
                IsMortgaged = mortgaged,
                RentByHouses = new[]
                {
                    baseRent, baseRent * 5, baseRent * 15,
                    baseRent * 37, baseRent * 46, baseRent * 55
                }
            };
        }

        private class FakeTile : ITileRentInfo
        {
            public string Name        { get; set; }
            public TileType Type      { get; set; }
            public ColorGroup Group   { get; set; }
            public bool IsMortgaged   { get; set; }
            public int HouseCount     { get; set; }
            public int BaseRent       { get; set; }
            public int[] RentByHouses { get; set; }
        }
    }
}
