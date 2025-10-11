using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace Assets.Tests.EditMode
{
    public class PlayerManager_Events_Tests
    {

        private GameObject host;
        private PlayerManager pm;

        [SetUp]
        public void Setup()
        {
            host = new GameObject("PM_TestHost");

            pm = host.AddComponent<PlayerManager>();


        }

        [TearDown]
        public void Teardown()
        {
            UObject.DestroyImmediate(host);
        }




        //verify each new player is recognized
        [Test]
        public void InitializePlayers_Raises_OnPlayerAdded_ForEach_NewPlayer()
        {
            int calls = 0;
            pm.OnPlayerAdded += (_, __) => calls++;

            pm.InitializePlayers(3);

            Assert.AreEqual(3, calls);
            Assert.AreEqual(3, pm.GetAllPlayers().Count);
            Assert.AreEqual("Player 1", pm.GetPlayer(0).GetPName());

            Assert.AreEqual("Player 2", pm.GetPlayer(1).GetPName());
            Assert.AreEqual("Player 3", pm.GetPlayer(2).GetPName());
        }


        //at start of game, removal and reindexing should be set up
        [Test]
        public void RemovePlayer_Raises_OnPlayerRemoved_And_Reindexes()
        {
            pm.InitializePlayers(3);


            int removedId = -1;
            pm.OnPlayerRemoved += id => removedId = id;

            bool ok = pm.RemovePlayer(1);

            Assert.IsTrue(ok);
            Assert.AreEqual(1, removedId);
            Assert.AreEqual(2, pm.GetAllPlayers().Count);

            Assert.AreEqual(0, pm.GetPlayer(0).GetId());
            Assert.AreEqual(1, pm.GetPlayer(1).GetId());
        }

        //invalid removals dont run events (should work with empty return statements built in)
        [Test]
        public void Remove_InvalidId_DoesNotRaiseEvent()
        {

            pm.InitializePlayers(1);
            int calls = 0;


            pm.OnPlayerRemoved += _ => calls++;

            bool ok = pm.RemovePlayer(99);

            Assert.IsFalse(ok);
            Assert.AreEqual(0, calls);
            Assert.AreEqual(1, pm.GetAllPlayers().Count);

        }

    }



}
