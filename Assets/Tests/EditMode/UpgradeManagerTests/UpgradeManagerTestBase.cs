using NUnit.Framework;
using Tests.EditMode;
using UnityEngine;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class UpgradeManagerTestBase : ManagerTestBase
    {
        protected Player TestPlayer;
        protected Player OtherPlayer;
        protected PropertySpaceData TestPropertyData;
        protected UpgradeManager TestManager;
        protected GameObject ManagerGameObject;

        [TearDown]
        protected virtual void TearDown()
        {
            if (ManagerGameObject != null)
                Object.DestroyImmediate(ManagerGameObject);

            if (TestPropertyData != null)
                Object.DestroyImmediate(TestPropertyData);

            if (TestPlayer != null)
                Object.DestroyImmediate(TestPlayer);

            if (OtherPlayer != null)
                Object.DestroyImmediate(OtherPlayer);
        }

        protected UpgradeManager CreateManager()
        {
            ManagerGameObject = new GameObject("UpgradeManagerTestObject");
            TestManager = ManagerGameObject.AddComponent<UpgradeManager>();
            return TestManager;
        }

        protected Player CreatePlayer(int startingMoney)
        {
            TestPlayer = ScriptableObject.CreateInstance<Player>();
            TestPlayer.SetMoney(startingMoney);
            return TestPlayer;
        }

        protected Player CreateOtherPlayer(int startingMoney)
        {
            OtherPlayer = ScriptableObject.CreateInstance<Player>();
            OtherPlayer.SetMoney(startingMoney);
            return OtherPlayer;
        }

        protected PropertySpaceData CreateProperty(
            int[] rentByUpgradeLevel,
            int upgradeCost = 50,
            int startingUpgradeLevel = 0,
            Player owner = null)
        {
            TestPropertyData = ScriptableObject.CreateInstance<PropertySpaceData>();
            TestPropertyData.researchFundingValues = rentByUpgradeLevel;
            TestPropertyData.dataPointCost = upgradeCost;
            TestPropertyData.SetUpgradeLevel(startingUpgradeLevel);

            if (owner != null)
            {
                TestPropertyData.SetOwner(owner);
            }

            return TestPropertyData;
        }
    }
}