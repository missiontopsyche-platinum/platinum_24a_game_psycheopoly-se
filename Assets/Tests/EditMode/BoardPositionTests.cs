using NUnit.Framework;
using UnityEngine;
using PsycheOpoly.Board;


namespace PsycheOpoly.Tests.EditMode
{

    public class BoardPositionTests
    {
        private BoardManager MakeManager(int size = 6)
        {
            var go = new GameObject("BoardManager Tests");
            var bm = go.AddComponent<BoardManager>();
            bm.InitializeBoard(size);
            return bm;
        }

        //Tests that player is in correct position
        [Test]
        public void GetSetPosition_DefaultsZero_ThenWraps()
        {
            var bm = MakeManager(6);
            const int pid = 42;

            Assert.AreEqual(0, bm.GetPlayerPosition(pid));

            bm.SetPlayerPosition(pid, 4);
            Assert.AreEqual(4, bm.GetPlayerPosition(pid));

            bm.SetPlayerPosition(pid, 999);
            Assert.AreEqual(999 % bm.BoardSize, bm.GetPlayerPosition(pid));
        }

        //Tests that player is in correct location after a move
        public void MovePlayer_WrapsMod_PositiveAndNegative()
        {
            var bm = MakeManager(6);
            const int pid = 1;

            bm.SetPlayerPosition(pid, 5);
            Assert.AreEqual(2, bm.MovePlayer(pid, 3));

            Assert.AreEqual(5, bm.MovePlayer(pid, -3));
        }
    }

}
