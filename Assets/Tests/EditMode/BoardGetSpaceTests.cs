using NUnit.Framework;
using UnityEngine;
using PsycheOpoly.Board;
using BoardSpace = PsycheOpoly.Board.Space;

namespace PsycheOpoly.Tests.EditMode
{

    public class BoardGetSpaceTests
    {
        private BoardManager MakeManager(int size = 5)
        {
            var go = new GameObject("BoardManagerTests");
            var bm = go.AddComponent<BoardManager>();
            bm.InitializeBoard(size);
            return bm;
        }

        //Tests That board spaces are numbered and generated correctly
        [Test]
        public void GetSpaceWraps_Positive_And_Negative()
        {
            var bm = MakeManager(5);
            var s0 = bm.GetSpace(0);
            var s5 = bm.GetSpace(5);
            var sNeg = bm.GetSpace(-5);

            Assert.AreSame(s0, s5);
            Assert.AreSame(s0, sNeg);
        }
    }

}
