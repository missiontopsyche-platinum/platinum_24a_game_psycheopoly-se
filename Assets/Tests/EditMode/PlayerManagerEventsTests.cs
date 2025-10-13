using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class PlayerManagerEventsTests
    {

        private GameObject gameObject;
        private PlayerManager playerManager;
        private PlayerEventChannel playerAddedChannel;
        private PlayerEventChannel playerRemovedChannel;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("PM_TestHost");
            playerManager = gameObject.AddComponent<PlayerManager>();
            playerAddedChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            playerRemovedChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            playerManager.playerAddedEventChannel = playerAddedChannel;
            playerManager.playerRemovedEventChannel = playerRemovedChannel;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameObject);
        }
        
        //verify each new player is recognized
        [Test]
        public void InitializePlayers_Raises_OnPlayerAdded_ForEach_NewPlayer()
        {
            int callbackCount = 0;

            void Listener(Player player)
            {
                callbackCount++;
            }

            playerAddedChannel.Subscribe(Listener);
            playerManager.InitializePlayers(3);

            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(3, playerManager.GetAllPlayers().Count);
            Assert.AreEqual("Player 1", playerManager.GetPlayer(0).GetPName());

            Assert.AreEqual("Player 2", playerManager.GetPlayer(1).GetPName());
            Assert.AreEqual("Player 3", playerManager.GetPlayer(2).GetPName());
        }


        //at start of game, removal and reindexing should be set up
        [Test]
        public void RemovePlayer_Raises_OnPlayerRemoved_And_Reindexes()
        {
            int removedId = -1;
            void Listener(Player player)
            {
                removedId = player.GetId();
            }
            
            playerRemovedChannel.Subscribe(Listener);
            
            playerManager.InitializePlayers(3);
            bool ok = playerManager.RemovePlayer(1);

            Assert.IsTrue(ok);
            Assert.AreEqual(1, removedId);
            Assert.AreEqual(2, playerManager.GetAllPlayers().Count);

            Assert.AreEqual(0, playerManager.GetPlayer(0).GetId());
            Assert.AreEqual(1, playerManager.GetPlayer(1).GetId());
        }

        //invalid removals dont run events (should work with empty return statements built in)
        [Test]
        public void Remove_InvalidId_DoesNotRaiseEvent()
        {
            int callbackCount = 0;

            void Listener(Player player)
            {
                callbackCount++;
            }
            
            playerRemovedChannel.Subscribe(Listener);
            
            playerManager.InitializePlayers(1);
            bool ok = playerManager.RemovePlayer(99);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, callbackCount);
            Assert.AreEqual(1, playerManager.GetAllPlayers().Count);

        }

    }



}
