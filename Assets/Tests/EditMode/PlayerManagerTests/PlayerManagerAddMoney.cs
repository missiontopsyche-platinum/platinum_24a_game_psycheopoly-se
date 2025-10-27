using UnityEngine;
using NUnit.Framework;

namespace Tests.EditMode.PlayerManagerTests
{
    public class PlayerManagerAddMoney : PlayerManagerTestBase
    {
        [Test]
        public void PlayerManager_AddMoney_PostiveNegative()
        {
            // Test specific setup
            playerManager.InitializePlayers(2);

            playerManager.AddMoney(0, 100);
            Assert.AreEqual(1600, playerManager.GetPlayer(0).GetMoney());

            playerManager.AddMoney(1, -100);
            Assert.AreEqual(1400, playerManager.GetPlayer(1).GetMoney());
        }
    }
}