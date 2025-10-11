using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class PlayerManagerEventsTests
    {

        private GameObject gameObject;
        private PlayerManager playerManager;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("PM_TestHost");

            playerManager = gameObject.AddComponent<PlayerManager>();


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
            int calls = 0;
            playerManager.OnPlayerAdded += (_, __) => calls++;

            playerManager.InitializePlayers(3);

            Assert.AreEqual(3, calls);
            Assert.AreEqual(3, playerManager.GetAllPlayers().Count);
            Assert.AreEqual("Player 1", playerManager.GetPlayer(0).GetPName());

            Assert.AreEqual("Player 2", playerManager.GetPlayer(1).GetPName());
            Assert.AreEqual("Player 3", playerManager.GetPlayer(2).GetPName());
        }


        //at start of game, removal and reindexing should be set up
        [Test]
        public void RemovePlayer_Raises_OnPlayerRemoved_And_Reindexes()
        {
            playerManager.InitializePlayers(3);


            int removedId = -1;
            playerManager.OnPlayerRemoved += id => removedId = id;

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

            playerManager.InitializePlayers(1);
            int calls = 0;


            playerManager.OnPlayerRemoved += _ => calls++;

            bool ok = playerManager.RemovePlayer(99);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(1, playerManager.GetAllPlayers().Count);

        }

    }



}
