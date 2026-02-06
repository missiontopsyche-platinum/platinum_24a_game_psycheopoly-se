using NUnit.Framework;
using System.Reflection;
using Tests.EditMode;
using UnityEngine;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class UpgradeManagerTestBase : ManagerTestBase
    {
        protected Player TestPlayer;
        protected PropertySpaceData TestPropertyData;
        protected OwnableSpaceTileAdapter TestAdapter;

        protected virtual void TearDown()
        {
            if (TestAdapter != null)
                Object.DestroyImmediate(TestAdapter.gameObject);

            if (TestPropertyData != null)
                Object.DestroyImmediate(TestPropertyData);

            if (TestPlayer != null)
                Object.DestroyImmediate(TestPlayer);
        }

        protected Player CreatePlayer(int startingMoney)
        {
            TestPlayer = ScriptableObject.CreateInstance<Player>();
            TestPlayer.SetMoney(startingMoney);
            return TestPlayer;
        }

        protected PropertySpaceData CreateProperty(
            int[] rentByUpgradeLevel,
            int upgradeCost = 50,
            int startingUpgradeLevel = 0)
        {
            TestPropertyData = ScriptableObject.CreateInstance<PropertySpaceData>();
            TestPropertyData.researchFundingValues = rentByUpgradeLevel;
            TestPropertyData.dataPointCost = upgradeCost;
            TestPropertyData.SetUpgradeLevel(startingUpgradeLevel);
            return TestPropertyData;
        }

        protected OwnableSpaceTileAdapter CreateAdapter(OwnableSpaceData data)
        {
            var go = new GameObject("TestOwnableTile");
            TestAdapter = go.AddComponent<OwnableSpaceTileAdapter>();

            var field = typeof(OwnableSpaceTileAdapter)
                .GetField("data", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(field, "OwnableSpaceTileAdapter.data field not found");
            field.SetValue(TestAdapter, data);

            return TestAdapter;
        }
    }
}
