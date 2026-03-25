using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Purchase;
using Assets.Scripts.Managers.Rules;
using Tests.EditMode; 

public class OwnableSpaceAdapterIntegrationTests : ManagerTestBase
{
    private GameObject spaceGO;
    private OwnableSpaceTileAdapter adapter;
    private PropertySpaceData spaceData;
    private Player buyer;
    private TestOwnership ownership;
    private TestRules rules;
    private PurchaseUtility utility;

    [SetUp]
    public void SetUp()
    {
        spaceData = ScriptableObject.CreateInstance<PropertySpaceData>();
        spaceData.buyPrice = 300; //real purchase price expected to be used

        if (spaceData.researchFundingValues == null || spaceData.researchFundingValues.Length == 0)
        {
            spaceData.researchFundingValues = new[] { 20, 40, 60, 80, 100, 120 };
        }
        else
        {
            spaceData.researchFundingValues[0] = 20;
            if (spaceData.researchFundingValues.Length > 1)
                spaceData.researchFundingValues[1] = 40;
        }

        //Create GameObject and adapter component
        spaceGO = new GameObject("OwnableSpaceTile");
        adapter = spaceGO.AddComponent<OwnableSpaceTileAdapter>();

        //use reflection to assign it in tests without changing runtime visibility
        var field = typeof(OwnableSpaceTileAdapter)
            .GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, "Could not find 'data' field on OwnableSpaceTileAdapter. " +
                                 "Make sure it exists and is named 'data'.");
        field.SetValue(adapter, spaceData);

        //Buyer and strategy setup
        buyer = ScriptableObject.CreateInstance<Player>();
        buyer.SetPName("Buyer");
        buyer.SetMoney(1000);  //enough to afford

        ownership = new TestOwnership();
        rules     = new TestRules();
        utility  = new PurchaseUtility();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestObjects(spaceGO);
        Object.DestroyImmediate(spaceData);
        Object.DestroyImmediate(buyer);
    }

    [Test]
    public void Strategy_Uses_BuyPrice_From_OwnableSpaceData_Via_Adapter()
    {
        //ask strategy for decision using adapter as the tile
        PurchaseEvaluation evaluation = utility.EvaluatePurchase(adapter, buyer, ownership, rules);

        //it should offer purchase and use the ScriptableObject buyPrice
        Assert.AreEqual(PurchaseFlow.OfferToPlayer, evaluation.Flow,
            "Expected strategy to offer purchase for unowned affordable tile.");

        Assert.IsTrue(evaluation.CanAfford,
            "Buyer should be able to afford the tile at the given price.");

        Assert.AreEqual(spaceData.buyPrice, evaluation.Price,
            "Strategy should use buyPrice from OwnableSpaceData via the adapter.");
    }

    //test doubles for rules and ownership

    private class TestRules : IRuleSet
    {
        public int RailroadBaseRent()      => 25;
        public int UtilityRentSingleMult() => 4;
        public int UtilityRentBothMult()   => 10;

        public int StreetsInGroup(ColorGroup g) =>
            (g == ColorGroup.Brown || g == ColorGroup.DarkBlue) ? 2 : 3;
        public int PlayerStartingMoney() => 1500;
        public int GOSalary()             => 200;
        public int JailFee()              => 50;
        public WinConditionType WinCondition() => WinConditionType.LastPlayerStanding;
        public int TargetMoney()          => 5000;
        public int TurnLimit()            => 20;
        public int MaxJailTurns()         => 3;
    }

    private class TestOwnership : IOwnershipService
    {
        //only care that the tile is unowned
        public Player GetOwner(ITileRentInfo tile) => null;

        public int CountOwnedInGroup(Player owner, ColorGroup group) => 0;
        public int CountRailroadsOwned(Player owner) => 0;
        public bool OwnsBothUtilities(Player owner) => false;
    }
}
