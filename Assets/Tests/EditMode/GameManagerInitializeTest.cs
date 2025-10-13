using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class GameManagerInitializeTest
    {
        private GameObject gameObject;
        private GameManager gameManager;
        private GameStateEventChannel gameStateEventChannel;
        
        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("GM_Events_Tests");
            gameManager = gameObject.AddComponent<GameManager>();
            gameStateEventChannel = ScriptableObject.CreateInstance<GameStateEventChannel>();
            gameManager.gameStateChangeChannel = gameStateEventChannel;
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
