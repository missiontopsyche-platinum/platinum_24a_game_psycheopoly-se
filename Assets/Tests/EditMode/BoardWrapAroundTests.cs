using NUnit.Framework;
using UnityEngine;
using PsycheOpoly.Board;

namespace PsycheOpoly.Tests.EditMode
{
    public class BoardWrapAroundTests
    {
        private BoardManager Make(int size = 6)
        {
            var go = new GameObject("BM");
            var bm = go.AddComponent<BoardManager>();
            bm.enabled = false; bm.enabled = true;
            bm.InitializeBoard(size);
            return bm;
        }

        //Checks if move wraps position correctly
        [Test]
        public void Move_wraps_forward_past_end()
        {
            var bm = Make(6);
            int pid = 1;
            bm.SetPlayerPosition(pid, 5);      
            var idx = bm.MovePlayer(pid, 2);   
            Assert.AreEqual(1, idx);
            Assert.AreEqual(1, bm.GetPlayerPosition(pid));
        }

        //Check if move wraps correctly with mutliple player loops
        [Test]
        public void Move_wraps_multiple_loops()
        {
            var bm = Make(6);
            int pid = 2;
            bm.SetPlayerPosition(pid, 0);
            var idx = bm.MovePlayer(pid, 14);  
            Assert.AreEqual(2, idx);
        }

        //Checks if logic can handle negative move
        [Test]
        public void Move_handles_negative_steps()
        {
            var bm = Make(6);
            int pid = 3;
            bm.SetPlayerPosition(pid, 1);
            var idx = bm.MovePlayer(pid, -3); 
            Assert.AreEqual(4, idx);
        }

        //Tests if GetSpace properly wraps code
        [Test]
        public void GetSpace_wraps_index()
        {
            var bm = Make(6);
            Assert.DoesNotThrow(() => bm.GetSpace(12)); 
            Assert.DoesNotThrow(() => bm.GetSpace(-7)); 
        }
    }
}
