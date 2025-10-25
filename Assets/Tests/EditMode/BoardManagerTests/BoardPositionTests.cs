using NUnit.Framework;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardPositionTests : BoardManagerTestBase
    {
        //Tests that player is in correct position
        [Test]
        public void GetSetPosition_DefaultsZero()
        {
            boardManager.InitializeBoard(6);
            const int pid = 42;

            Assert.AreEqual(0, boardManager.GetPlayerPosition(pid));

            boardManager.SetPlayerPosition(pid, 4);
            Assert.AreEqual(4, boardManager.GetPlayerPosition(pid));

        }

        [Test]
        public void SetPositionOutOfBoundsThrowsError()
        {
            boardManager.InitializeBoard(6);
            const int pid = 1;


            // Changing to test that this instead throws the appropriate error if setting out of range
            Assert.Throws<System.ArgumentOutOfRangeException>(() => boardManager.SetPlayerPosition(pid, 999));
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