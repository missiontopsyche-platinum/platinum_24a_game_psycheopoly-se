using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class GameManagerInitializeTest
    {
        GameObject gameObject;
        GameManager gameManager;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("GM_Init_Test");

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
        public void Initialize_SetsState_ToInitializing()
        {
            Assert.AreEqual(GameState.None, gameManager.gameState);
            gameManager.Initialize();
            Assert.AreEqual(GameState.Initializing, gameManager.gameState);
        }
    }
}
