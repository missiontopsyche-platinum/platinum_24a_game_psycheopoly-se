using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class UpgradeManagerTests : UpgradeManagerTestBase
    {
        [Test]
        public void TryHandleUpgradeFalse_WhenOwnerOrTileNull()
        {
            var mgr = CreateManager();

            var result = mgr.TryHandleUpgrade(null, null, null, out _);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryHandleUpgradeFalse_WhenDecisionNotAllowedDueToZeroCost()
        {
            var mgr = CreateManager();

            var player = CreatePlayer(999);
            var pd = CreateProperty(new[] { 10, 20, 30 }, upgradeCost: 0, owner: player);

            var allSpaces = new OwnableSpaceData[] { pd };

            var result = mgr.TryHandleUpgrade(player, pd, allSpaces, out var decision);

            Assert.IsFalse(result);
            Assert.IsFalse(decision.Allowed);
            Assert.AreEqual(UpgradeFailReason.InvalidUpgradeCost, decision.FailReason);
        }

        [Test]
        public void TryHandleUpgradeFalse_WhenPlayerDoesNotOwnProperty()
        {
            var mgr = CreateManager();

            var owner = CreatePlayer(999);
            var otherPlayer = CreateOtherPlayer(999);
            var pd = CreateProperty(new[] { 10, 20, 30 }, upgradeCost: 50, owner: otherPlayer);

            var allSpaces = new OwnableSpaceData[] { pd };

            var result = mgr.TryHandleUpgrade(owner, pd, allSpaces, out var decision);

            Assert.IsFalse(result);
            Assert.IsFalse(decision.Allowed);
            Assert.AreEqual(UpgradeFailReason.NotOwner, decision.FailReason);
        }

        [Test]
        public void TryHandleUpgradeTrue_WhenUpgradeAllowed()
        {
            var mgr = CreateManager();
            var player = CreatePlayer(999);

            var pd1 = CreateProperty(new[] { 10, 20, 30 }, upgradeCost: 50, startingUpgradeLevel: 0, owner: player);

            var pd2 = ScriptableObject.CreateInstance<PropertySpaceData>();
            pd2.SetResearchFundingValues(new[] { 10, 20, 30 });
            pd2.SetDataPointCost(50);
            pd2.SetUpgradeLevel(0);
            pd2.SetOwner(player);

            var pd3 = ScriptableObject.CreateInstance<PropertySpaceData>();
            pd3.SetResearchFundingValues(new[] { 10, 20, 30 });
            pd3.SetDataPointCost(50);
            pd3.SetUpgradeLevel(0);
            pd3.SetOwner(player);

            var allSpaces = new OwnableSpaceData[] { pd1, pd2, pd3 };

            var result = mgr.TryHandleUpgrade(player, pd1, allSpaces, out var decision);

            Assert.IsTrue(result);
            Assert.IsTrue(decision.Allowed);
            Assert.AreEqual(50, decision.Cost);
            Assert.AreEqual(1, pd1.GetCurrentUpgradeLevel());

            Object.DestroyImmediate(pd2);
            Object.DestroyImmediate(pd3);
        }
    }
}