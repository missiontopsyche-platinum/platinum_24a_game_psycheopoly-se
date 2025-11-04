using NUnit.Framework;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardWrapAroundTests : BoardManagerTestBase
    {
        [Test]
        public void Move_wraps_forward_past_end()
        {
            boardManager.InitializeBoard(6);
            int pid = 1;
            boardManager.SetPlayerPosition(pid, 5);      
            boardManager.MovePlayer(new MovePlayerEvent(pid, 2));   
            Assert.AreEqual(1, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void Move_wraps_multiple_loops()
        {
            boardManager.InitializeBoard(6);
            int pid = 2;
            boardManager.SetPlayerPosition(pid, 0);
            boardManager.MovePlayer(new MovePlayerEvent(pid, 14));  
            Assert.AreEqual(2, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void Move_handles_negative_steps()
        {
            boardManager.InitializeBoard(6);
            int pid = 3;
            boardManager.SetPlayerPosition(pid, 1);
            boardManager.MovePlayer(new MovePlayerEvent(pid, -3)); 
            Assert.AreEqual(4, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void GetSpace_wraps_index()
        {
            boardManager.InitializeBoard(6);
            Assert.DoesNotThrow(() => boardManager.GetSpace(12)); 
            Assert.DoesNotThrow(() => boardManager.GetSpace(-7)); 
        }
    }
}
