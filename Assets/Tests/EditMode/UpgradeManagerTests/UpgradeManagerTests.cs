using NUnit.Framework;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class UpgradeManagerTests : UpgradeManagerTestBase
    {
        [Test]
        public void TryHandleUpgrade_ReturnsFalse_WhenOwnerOrTileNull()
        {
            var mgr = new UpgradeManager();

            Assert.IsFalse(mgr.TryHandleUpgrade(null, null, out _));
        }

        [Test]
        public void TryHandleUpgrade_ReturnsFalse_WhenDecisionNotAllowed_DueToZeroCost()
        {
            var mgr = new UpgradeManager();

            var player = CreatePlayer(999);
            var pd = CreateProperty(new[] { 10, 20, 30 }, upgradeCost: 50);
            var adapter = CreateAdapter(pd);

            var result = mgr.TryHandleUpgrade(player, adapter, out var decision);

            Assert.IsFalse(result);
            Assert.IsFalse(decision.Allowed);
        }
    }
}

