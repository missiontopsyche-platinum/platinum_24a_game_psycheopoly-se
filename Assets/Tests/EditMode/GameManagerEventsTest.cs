using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class GameManagerEventsTest
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
        public void GameStateChanged_Fires_On_Initialize()
        {
            int callbackCount = 0;
            GameStateChange gameStateChange = new GameStateChange();

            void Listener(GameStateChange stateChange)
            {
                callbackCount++;
                gameStateChange = stateChange;
            }
            
            gameStateEventChannel.Subscribe(Listener);

            gameManager.Initialize();

            //this is none > initialzing
            Assert.AreEqual(1, callbackCount); 
            Assert.AreEqual(GameState.None, gameStateChange.previous);
            Assert.AreEqual(GameState.Initializing, gameStateChange.current);
        }
    }
}
