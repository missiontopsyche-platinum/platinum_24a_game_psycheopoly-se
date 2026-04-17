using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class PropertySpaceDataTests : UpgradeManagerTestBase
    {
       [Test]
        public void TryUpgradeIncrementsUntilMax_ThenFails()
        {
            var pd = ScriptableObject.CreateInstance<PropertySpaceData>();

            pd.SetResearchFundingValues(new[] { 10, 20, 30 });
            pd.SetUpgradeCostByLevel(new[] { 10, 20, 30 });
            pd.SetDataPointCost(10);
            pd.isMortgaged = false;
            pd.SetUpgradeLevel(0);

            Assert.AreEqual(0, pd.GetCurrentUpgradeLevel());
            Assert.AreEqual(2, pd.MaxUpgradeLevel);
            Assert.IsFalse(pd.IsMaxed);

            Assert.IsTrue(pd.TryUpgrade());
            Assert.AreEqual(1, pd.GetCurrentUpgradeLevel());

            Assert.IsTrue(pd.TryUpgrade());
            Assert.AreEqual(2, pd.GetCurrentUpgradeLevel());
            Assert.IsTrue(pd.IsMaxed);

            Assert.IsFalse(pd.TryUpgrade());
            Assert.AreEqual(2, pd.GetCurrentUpgradeLevel());

            Object.DestroyImmediate(pd);
        }

        [Test]
        public void SetUpgradeLevel_ClampsWithinBounds()
        {
            var pd = ScriptableObject.CreateInstance<PropertySpaceData>();
            pd.SetResearchFundingValues(new[] { 1, 2, 3, 4 });
            pd.SetUpgradeCostByLevel(new[] { 1, 2, 3, 4 });
            pd.SetDataPointCost(10);

            pd.SetUpgradeLevel(-999);
            Assert.AreEqual(0, pd.GetCurrentUpgradeLevel());

            pd.SetUpgradeLevel(999);
            Assert.AreEqual(pd.MaxUpgradeLevel, pd.GetCurrentUpgradeLevel());

            pd.SetUpgradeLevel(2);
            Assert.AreEqual(2, pd.GetCurrentUpgradeLevel());

            Object.DestroyImmediate(pd);
        }

        [Test]
        public void TryUpgradeFail_WhenDataPointCostNonPositive()
        {
            var pd = ScriptableObject.CreateInstance<PropertySpaceData>();
            pd.SetResearchFundingValues(new[] { 1, 2, 3 });
            pd.SetUpgradeCostByLevel(new[] { 1, 2, 3 });
            pd.SetDataPointCost(0);

            Assert.IsFalse(pd.TryUpgrade());
            Assert.AreEqual(0, pd.GetCurrentUpgradeLevel());

            Object.DestroyImmediate(pd);
        }
    }
}