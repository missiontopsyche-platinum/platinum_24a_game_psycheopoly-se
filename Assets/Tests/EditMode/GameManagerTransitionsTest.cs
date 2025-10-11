using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode
{

 
    public class GameManagerTransitionsTest
    {
        GameObject gameObject;
        GameManager gameManager;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("GM_Transition_Test");
            gameManager = gameObject.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
            }

        }

        [Test]
        public void EndGame_FromNone_IsBlocked()
        {
            gameManager.EndGame();
            LogAssert.Expect("[Game Manager] transition not allowed from : None to GameOver");
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
