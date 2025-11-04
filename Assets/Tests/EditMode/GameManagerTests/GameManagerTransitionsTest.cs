using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTransitionsTest : GameManagerTestBase
    {
        [Test]
        public void EndGame_FromNone_IsBlocked()
        {
            LogAssert.Expect("Test [Level: Warn] " +
                "[Category: Gameplay] " +
                "[Event Name: GameManager.SetState] " +
                "[Message: Illegal transition: None -> GameOver]");
            gameManager.EndGame();
            Assert.AreEqual(GameState.None, gameManager.gameState);
        }

        [Test]
        public void Initialize_AllowsTransition_FromNone()
        {
            gameManager.Initialize();
            Assert.AreEqual(GameState.Initializing, gameManager.gameState);
        }
    }
}
