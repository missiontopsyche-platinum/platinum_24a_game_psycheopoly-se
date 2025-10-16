using NUnit.Framework;
using UnityEngine;
using PsycheOpoly.Board;
using PsycheOpoly.Events;

namespace PsycheOpoly.Tests.EditMode
{

    public class BoardEventHandlingTests : BoardManagerTestBase
    {
        //Tests that Manager responds to player moves
        [Test]
        public void EnabledManager_Responds_to_PlayerMovedEvent()
        {
            boardManager.InitializeBoard(6);
            const int pid = 7;

            boardManager.SetPlayerPosition(pid, 0);
            // TODO refactor to use EventChannel instead of C# events
            GameEvents.RaisePlayerMoved(pid, 3);
            Assert.AreEqual(3, boardManager.GetPlayerPosition(pid));
        }

        //Tests events can resubscribe 
        [Test]
        public void Disabled_Manager_DoesNotRespond_then_Resubscribe()
        {
            // TODO review if this behaviour is needed with EventChannels
            boardManager.InitializeBoard(6);
            const int pid = 8;

            boardManager.SetPlayerPosition(pid, 0);
            boardManager.enabled = false;  //Triggers OnDisable() to unsubscribe
            GameEvents.RaisePlayerMoved(pid, 4);
            Assert.AreEqual(0, boardManager.GetPlayerPosition(pid));

            boardManager.enabled = true; //Triggers OnEnable() to resubscribe
            GameEvents.RaisePlayerMoved(pid, 2);
            Assert.AreEqual(2, boardManager.GetPlayerPosition(pid));
        }
    }

}
