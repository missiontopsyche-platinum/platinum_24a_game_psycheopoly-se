using NUnit.Framework;

namespace PsycheOpoly.Tests.EditMode
{

    public class BoardGetSpaceTests : BoardManagerTestBase
    {
        //Tests That board spaces are numbered and generated correctly
        [Test]
        public void GetSpaceWraps_Positive_And_Negative()
        {
            boardManager.InitializeBoard(5);
            var space0 = boardManager.GetSpace(0);
            var space5 = boardManager.GetSpace(5);
            var spaceNegative = boardManager.GetSpace(-5);

            Assert.AreSame(space0, space5);
            Assert.AreSame(space0, spaceNegative);
        }
    }

}
