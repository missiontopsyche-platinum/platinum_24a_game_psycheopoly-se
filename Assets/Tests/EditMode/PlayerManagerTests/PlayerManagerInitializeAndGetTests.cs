using System.Collections.Generic;
using Logging;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.EditMode.PlayerManagerTests
{
    public class PlayerManagerInitializeAndGetTests : PlayerManagerTestBase
    {
        [Test]
        public void PlayerManager_CanInitialize2Players()
        {
            playerManager.InitializePlayers(GeneratePlayerConfigs("Player 1", "Player 2"));

            List<Player> players = playerManager.GetAllPlayers();
        
            Assert.AreEqual(2, players.Count);
            Assert.AreEqual("Player 1", players[0].GetPName());
            Assert.AreEqual("Player 2", players[1].GetPName());
        }
    
        [Test]
        public void PlayerManager_CanInitialize3Players()
        {
            playerManager.InitializePlayers(
                GeneratePlayerConfigs("Player 1", "Player 2", "Player 3"));

            List<Player> players = playerManager.GetAllPlayers();
        
            Assert.AreEqual(3, players.Count);
            Assert.AreEqual("Player 1", players[0].GetPName());
            Assert.AreEqual("Player 2", players[1].GetPName());
            Assert.AreEqual("Player 3", players[2].GetPName());
        }
    
        [Test]
        public void PlayerManager_CanInitialize4Players()
        {
            playerManager.InitializePlayers(
                GeneratePlayerConfigs("Player 1", "Player 2", "Player 3", "Player 4"));

            List<Player> players = playerManager.GetAllPlayers();
        
            Assert.AreEqual(4, players.Count);
            Assert.AreEqual("Player 1", players[0].GetPName());
            Assert.AreEqual("Player 2", players[1].GetPName());
            Assert.AreEqual("Player 3", players[2].GetPName());
            Assert.AreEqual("Player 4", players[3].GetPName());
        }

        [Test]
        public void PlayerManager_CanGetPlayerById()
        {
            playerManager.InitializePlayers(
                GeneratePlayerConfigs("Player 1", "Player 2", "Player 3", "Player 4"));

            Player player = playerManager.GetPlayer(2);
        
            Assert.AreEqual(2, player.GetId());
            Assert.AreEqual("Player 3", player.GetPName());
        }

        [Test]
        public void PlayerManager_GetByInvalidIdReturnsNull()
        {
            playerManager.InitializePlayers(GeneratePlayerConfigs("Player 1", "Player 2"));

            var pattern = CreateRegexLogPattern("Error", "Gameplay", "PlayerManager.GetPlayer", "Attempted access of playerID out of bounds: 3");
            LogAssert.Expect(UnityEngine.LogType.Error, pattern);
            Assert.IsNull(playerManager.GetPlayer(3));
        }
    }
}

