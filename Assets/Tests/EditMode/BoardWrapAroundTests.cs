using NUnit.Framework;

namespace PsycheOpoly.Tests.EditMode
{
    public class BoardWrapAroundTests : BoardManagerTestBase
    {
        [Test]
        public void Move_wraps_forward_past_end()
        {
            boardManager.InitializeBoard(6);
            int pid = 1;
            boardManager.SetPlayerPosition(pid, 5);      
            var idx = boardManager.MovePlayer(pid, 2);   
            Assert.AreEqual(1, idx);
            Assert.AreEqual(1, boardManager.GetPlayerPosition(pid));
        }

        [Test]
        public void Move_wraps_multiple_loops()
        {
            boardManager.InitializeBoard(6);
            int pid = 2;
            boardManager.SetPlayerPosition(pid, 0);
            var idx = boardManager.MovePlayer(pid, 14);  
            Assert.AreEqual(2, idx);
        }

        [Test]
        public void Move_handles_negative_steps()
        {
            boardManager.InitializeBoard(6);
            int pid = 3;
            boardManager.SetPlayerPosition(pid, 1);
            var idx = boardManager.MovePlayer(pid, -3); 
            Assert.AreEqual(4, idx);
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