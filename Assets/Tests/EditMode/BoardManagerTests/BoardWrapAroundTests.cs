using NUnit.Framework;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardWrapAroundTests : BoardManagerTestBase
    {
        [Test]
        public void Move_wraps_forward_past_end()
        {
            boardManager.InitializeBoard();
            int pid = 1;
            boardManager.SetPlayerPosition(pid, 39);      
            boardManager.MovePlayer(new MovePlayerEvent(pid, 2));   
            Assert.AreEqual(1, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void Move_wraps_multiple_loops()
        {
            boardManager.InitializeBoard();
            int pid = 2;
            boardManager.SetPlayerPosition(pid, 0);
            boardManager.MovePlayer(new MovePlayerEvent(pid, 82));  
            Assert.AreEqual(2, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void Move_handles_negative_steps()
        {
            boardManager.InitializeBoard();
            int pid = 3;
            boardManager.SetPlayerPosition(pid, 1);
            boardManager.MovePlayer(new MovePlayerEvent(pid, -3)); 
            Assert.AreEqual(38, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void GetSpace_wraps_index()
        {
            boardManager.InitializeBoard();
            Assert.DoesNotThrow(() => boardManager.GetSpace(12)); 
            Assert.DoesNotThrow(() => boardManager.GetSpace(-7)); 
        }
    }
}
