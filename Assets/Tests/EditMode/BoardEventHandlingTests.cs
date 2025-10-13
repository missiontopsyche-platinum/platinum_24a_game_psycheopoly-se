using NUnit.Framework;
using UnityEngine;
using PsycheOpoly.Board;
using PsycheOpoly.Events;

namespace PsycheOpoly.Tests.EditMode
{

    public class BoardEventHandlingTests
    {
        private BoardManager MakeManager(int size = 6)
        {
            var go = new GameObject("BoardManagerTests");
            var bm = go.AddComponent<BoardManager>();

            bm.enabled = false;
            bm.enabled = true;

            bm.InitializeBoard(size);
            return bm;
        }

        //Tests that Manager responds to player moves
        [Test]
        public void EnabledManager_Responds_to_PlayerMovedEvent()
        {
            var bm = MakeManager(6);
            const int pid = 7;

            bm.SetPlayerPosition(pid, 0);
            GameEvents.RaisePlayerMoved(pid, 3);
            Assert.AreEqual(3, bm.GetPlayerPosition(pid));
        }

        //Tests events can resubscribe 
        [Test]
        public void DIsabled_Manager_DoesNotRespond_then_Resubscribe()
        {
            var bm = MakeManager(6);
            const int pid = 8;

            bm.SetPlayerPosition(pid, 0);
            bm.enabled = false;  //Triggers OnDisable() to unsubscribe
            GameEvents.RaisePlayerMoved(pid, 4);
            Assert.AreEqual(0, bm.GetPlayerPosition(pid));

            bm.enabled = true; //Triggers OnEnable() to resubscribe
            GameEvents.RaisePlayerMoved(pid, 2);
            Assert.AreEqual(2, bm.GetPlayerPosition(pid));
        }
    }

}
