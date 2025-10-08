using NUnit.Framework;
using UnityEngine;
using PsycheOpoly.Board;
using BoardSpace = PsycheOpoly.Board.Space;

namespace PsycheOpoly.Tests.EditMode
{

    public class BoardInitializationTests
    {
        private BoardManager MakeManager()
        {
            var go = new GameObject("BoardManagerTests");
            return go.AddComponent<BoardManager>();
        } 

        [Test]
        public void InitializeBoard_BuildsArray_GoAtZero_HasMix()
        {
            var bm = MakeManager();
            bm.InitializeBoard(8);

            Assert.AreEqual(8, bm.BoardSize);
            Assert.IsInstanceOf<GoSpace>(bm.GetSpace(0));

            bool hasProp = false, hasChance = false;
            for(int i = 1; i < bm.BoardSize; i++)
            {
                var s = bm.GetSpace(i);
                if(s is PropertySpace) hasProp = true;
                if(s is ChanceSpace) hasChance = true;
            }

            Assert.IsTrue(hasProp, "Expected at least one Property Space");
            Assert.IsTrue(hasChance, "Expected at least one chance space");
        }

    }

}