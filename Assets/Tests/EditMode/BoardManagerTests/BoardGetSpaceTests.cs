using NUnit.Framework;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardGetSpaceTests : BoardManagerTestBase
    {
        //Tests That board spaces are numbered and generated correctly
        [Test]
        public void GetSpaceWraps_Positive_And_Negative()
        {
            boardManager.InitializeBoard();
            var space0 = boardManager.GetSpace(0);
            var space40 = boardManager.GetSpace(40);
            var spaceNegative = boardManager.GetSpace(-40);

            Assert.AreSame(space0, space40);
            Assert.AreSame(space0, spaceNegative);
        }
    }
}