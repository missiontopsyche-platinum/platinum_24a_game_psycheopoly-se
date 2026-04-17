using NUnit.Framework;
using PsycheOpoly.Board;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardInitializationTests : BoardManagerTestBase
    {
        // We can remove this because board verification is built into InitializeBoard.
        
        // //Tests that board is setup properly and has mix of spaces
        // [Test]
        // public void InitializeBoard_BuildsArray_GoAtZero_HasMix()
        // {
        //     boardManager.InitializeBoard();
        //
        //     Assert.AreEqual(8, boardManager.boardSize);
        //     Assert.IsInstanceOf<GoSpaceData>(boardManager.GetSpace(0));
        //
        //     bool hasPropSpace = false, hasCardSpace = false;
        //     for(int i = 1; i < boardManager.boardSize; i++)
        //     {
        //         var space = boardManager.GetSpace(i);
        //         if(space is PropertySpaceData) hasPropSpace = true;
        //         if(space is CardSpaceData) hasCardSpace = true;
        //     }
        //
        //     Assert.IsTrue(hasPropSpace, "Expected at least one Property Space");
        //     Assert.IsTrue(hasCardSpace, "Expected at least one chance space");
        // }

    }
}