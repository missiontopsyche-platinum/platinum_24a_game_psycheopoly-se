using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode
{
    public class PlayerManagerTests
    {
        private GameObject gameObject;
        private PlayerManager playerManager;
        
        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            playerManager = gameObject.AddComponent<PlayerManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void PlayerManager_CanInitialize2Players()
        {
            playerManager.InitializePlayers(2);

            List<Player> players = playerManager.GetAllPlayers();
            
            Assert.AreEqual(2, players.Count);
            Assert.AreEqual("Player 1", players[0].GetPName());
            Assert.AreEqual("Player 2", players[1].GetPName());
        }
        
        [Test]
        public void PlayerManager_CanInitialize3Players()
        {
            playerManager.InitializePlayers(3);

            List<Player> players = playerManager.GetAllPlayers();
            
            Assert.AreEqual(3, players.Count);
            Assert.AreEqual("Player 1", players[0].GetPName());
            Assert.AreEqual("Player 2", players[1].GetPName());
            Assert.AreEqual("Player 3", players[2].GetPName());
        }
        
        [Test]
        public void PlayerManager_CanInitialize4Players()
        {
            playerManager.InitializePlayers(4);

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
            playerManager.InitializePlayers(4);

            Player player = playerManager.GetPlayer(2);
            
            Assert.AreEqual(2, player.GetId());
            Assert.AreEqual("Player 3", player.GetPName());
        }

        [Test]
        public void PlayerManager_GetByInvalidIdReturnsNull()
        {
            playerManager.InitializePlayers(2);
            
            LogAssert.Expect("PlayerManager: GetPlayer " +
                             "attempted access of playerID out" +
                             "of bounds: 3");
            Assert.IsNull(playerManager.GetPlayer(3));
        }
    }
}

