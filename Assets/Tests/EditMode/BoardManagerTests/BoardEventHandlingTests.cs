using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardEventHandlingTests : BoardManagerTestBase
    {
        //Tests that Manager responds to player moves
        [Test]
        public void EnabledManager_Responds_to_PlayerMovedEvent()
        {
            boardManager.InitializeBoard(6);
            const int pid = 7;

            logger.Trace("BoardEventHandlingTests.EnabledManager_Responds_to_PlayerMovedEvent", 
                $"Before Set Player, GameObject = {gameObject}");
            boardManager.SetPlayerPosition(pid, 0);
            logger.Trace("BoardEventHandlingTests.EnabledManager_Responds_to_PlayerMovedEvent", 
                $"Before Event, enabled={boardManager.enabled}");

            
            boardManager.movePlayerChannel?.RaiseEvent(new MovePlayerEvent(pid, 3));
            logger.Trace("BoardEventHandlingTests.EnabledManager_Responds_to_PlayerMovedEvent",
                $"{pid} Pos: {boardManager.GetPlayerPosition(pid)}");
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
            boardManager.movePlayerChannel?.RaiseEvent(new MovePlayerEvent(pid, 4));
            logger.Trace("BoardEventHandlingTests.Disabled",
                $"{pid} Pos: {boardManager.GetPlayerPosition(pid)}");
            Assert.AreEqual(0, boardManager.GetPlayerPosition(pid));

            boardManager.enabled = true; //Triggers OnEnable() to resubscribe
            boardManager.movePlayerChannel?.RaiseEvent(new MovePlayerEvent(pid, 2));
            Assert.AreEqual(2, boardManager.GetPlayerPosition(pid));
        }
    }
}
