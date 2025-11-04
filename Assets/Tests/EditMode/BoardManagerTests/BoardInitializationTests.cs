using NUnit.Framework;
using PsycheOpoly.Board;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardInitializationTests : BoardManagerTestBase
    {
        //Tests that board is setup properly and has mix of spaces
        [Test]
        public void InitializeBoard_BuildsArray_GoAtZero_HasMix()
        {
            boardManager.InitializeBoard(8);

            Assert.AreEqual(8, boardManager.boardSize);
            Assert.IsInstanceOf<GoSpace>(boardManager.GetSpace(0));

            bool hasProp = false, hasChance = false;
            for(int i = 1; i < boardManager.boardSize; i++)
            {
                var space = boardManager.GetSpace(i);
                if(space is PropertySpace) hasProp = true;
                if(space is ChanceSpace) hasChance = true;
            }

            Assert.IsTrue(hasProp, "Expected at least one Property Space");
            Assert.IsTrue(hasChance, "Expected at least one chance space");
        }

    }
}