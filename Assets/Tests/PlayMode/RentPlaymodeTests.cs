using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;

public class RentPlaymodeTests
{
    // creating a mock tile for testing based on the ITileRentInfo
    private class MockStreet : ITileRentInfo
    {
        public string Name { get; }
        public TileType Type => TileType.Street;
        public ColorGroup Group { get; }
        public bool IsMortgaged { get; }
        public int HouseCount { get; }
        public int BaseRent { get; }
        public int[] RentByHouses { get; }

        public MockStreet(
            string name,
            ColorGroup group,
            int baseRent,
            int houses = 0,
            bool mortgaged = false,
            int[] rentTable = null)
        {
            Name = name;
            Group = group;
            BaseRent = baseRent;
            HouseCount = houses;
            IsMortgaged = mortgaged;
            RentByHouses = rentTable ?? new int[0];
        }
    }

    [UnityTest]
    public IEnumerator RentManager_ComputesCorrectRent_Playmode()
    {
        var go = new GameObject("RentManager");
        var rentManager = go.AddComponent<RentManager>();

        var tenant = ScriptableObject.CreateInstance<Player>();
        var owner = ScriptableObject.CreateInstance<Player>();
        tenant.SetMoney(1000);
        owner.SetMoney(1000);

        //ownership linked to the adapters
        go.AddComponent<OwnershipServiceAdapter>();
        go.AddComponent<EconomyAdapter>();

        var tile = new MockStreet("TestProp", ColorGroup.Red, baseRent: 10);

        rentManager.SetOwner(tile, owner);
        rentManager.TryChargeRent(tenant, tile, diceTotal: 0);


        yield return null;

        // isequal seciton
        Assert.AreEqual(990, tenant.GetMoney());
        Assert.AreEqual(1010, owner.GetMoney());
    }
}
