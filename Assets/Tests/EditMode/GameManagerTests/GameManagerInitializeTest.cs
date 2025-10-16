using NUnit.Framework;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerInitializeTest : GameManagerTestBase
    {
        [Test]
        public void Initialize_SetsState_ToInitializing()
        {
            Assert.AreEqual(GameState.None, gameManager.gameState);
            gameManager.Initialize();
            Assert.AreEqual(GameState.Initializing, gameManager.gameState);
        }
    }
}
