using UnityEngine;
using NUnit.Framework;

namespace Tests.EditMode.PlayerManagerTests
{
    public class PlayerManagerPassedGoTests : PlayerManagerTestBase
    {
        /// <summary>
        /// Tests that the playermanager receives the passedGoEvent call
        /// and Adds money to the player
        /// </summary>
        [Test]
        public void PlayerManager_ReceivesGo()
        {
            //Some test specific setup
            playerManager.InitializePlayers(1);
            playerManager.passedGoChannel.Subscribe(playerManager.PassedGo);

            const int pid = 0;

            playerManager.passedGoChannel?.RaiseEvent(pid);

            Assert.AreEqual(1700, playerManager.GetPlayer(pid).GetMoney());
        }
    }
}

