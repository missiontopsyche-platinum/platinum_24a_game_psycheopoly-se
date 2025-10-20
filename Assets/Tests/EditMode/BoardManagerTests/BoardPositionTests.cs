using NUnit.Framework;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardPositionTests : BoardManagerTestBase
    {
        //Tests that player is in correct position
        [Test]
        public void GetSetPosition_DefaultsZero_ThenWraps()
        {
            boardManager.InitializeBoard(6);
            const int pid = 42;

            Assert.AreEqual(0, boardManager.GetPlayerPosition(pid));

            boardManager.SetPlayerPosition(pid, 4);
            Assert.AreEqual(4, boardManager.GetPlayerPosition(pid));

            boardManager.SetPlayerPosition(pid, 999);
            Assert.AreEqual(999 % boardManager.BoardSize, boardManager.GetPlayerPosition(pid));
        }

        //Tests that player is in correct location after a move
        [Test]
        public void MovePlayer_WrapsMod_PositiveAndNegative()
        {
            boardManager.InitializeBoard(6);
            const int pid = 1;

            boardManager.SetPlayerPosition(pid, 5);
            boardManager.MovePlayer(new MovePlayerEvent(pid, 3));
            Assert.AreEqual(2, boardManager.GetPlayerPosition(pid));
            boardManager.MovePlayer(new MovePlayerEvent(pid, -3));
            Assert.AreEqual(5, boardManager.GetPlayerPosition(pid));
        }
    }
}