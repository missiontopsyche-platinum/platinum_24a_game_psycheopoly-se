using UnityEngine;

namespace Tests.EditMode.UpgradeManagerTests
{
    using NUnit.Framework;

    public class PropertySpaceDataTests : UpgradeManagerTestBase
    {
        [Test]
        public void TryUpgrade_IncrementsUntilMax_ThenFails()
        {
            var pd = new PropertySpaceData(null, upgradeCost: 25);
            int[] testArr = new[] { 10, 20, 30 };
            pd.SetDataPointCost(10);
            pd.SetUpgradeCostByLevel(testArr);
            pd.SetResearchFundingValues(testArr);

            Assert.AreEqual(0, pd.GetCurrentUpgradeLevel());
            Assert.IsFalse(pd.IsMaxed);

            Assert.IsTrue(pd.TryUpgrade());
            Assert.AreEqual(1, pd.GetCurrentUpgradeLevel());

            Assert.IsTrue(pd.TryUpgrade());
            Assert.AreEqual(2, pd.GetCurrentUpgradeLevel());
            Assert.IsTrue(pd.IsMaxed);

            Assert.IsFalse(pd.TryUpgrade());
            Assert.AreEqual(2, pd.GetCurrentUpgradeLevel());
        }

        [Test]
        public void SetUpgradeLevel_ClampsWithinBounds()
        {
            var pd = new PropertySpaceData(new[] { 1, 2, 3, 4 }, upgradeCost: 10);

            pd.SetUpgradeLevel(-999);
            Assert.AreEqual(0, pd.GetCurrentUpgradeLevel());

            pd.SetUpgradeLevel(999);
            Assert.AreEqual(pd.MaxUpgradeLevel, pd.GetCurrentUpgradeLevel());

            pd.SetUpgradeLevel(2);
            Assert.AreEqual(2, pd.GetCurrentUpgradeLevel());
        }

        [Test]
        public void TryUpgrade_Fails_WhenDataPointCostNonPositive()
        {
            var pd = new PropertySpaceData(new[] { 1, 2, 3 }, upgradeCost: 0);

            Assert.IsFalse(pd.TryUpgrade());
            Assert.AreEqual(0, pd.GetCurrentUpgradeLevel());
        }
    }

}