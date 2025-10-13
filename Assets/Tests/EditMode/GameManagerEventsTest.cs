using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class GameManagerEventsTest
    {
        private GameObject gameObject;
        private GameManager gameManager;
        private GameStateChangedEventChannel gameStateEventChannel;
        
        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("GM_Events_Tests");
            gameManager = gameObject.AddComponent<GameManager>();
            gameStateEventChannel = ScriptableObject.CreateInstance<GameStateChangedEventChannel>();
            gameManager.gameStateChangedChannel = gameStateEventChannel;
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
            GameStateChangedEvent gameStateChange = new GameStateChangedEvent(GameState.None, GameState.None);

            void Listener(GameStateChangedEvent stateChange)
            {
                callbackCount++;
                gameStateChange = stateChange;
            }
            
            gameStateEventChannel.Subscribe(Listener);

            gameManager.Initialize();

            //this is none > initialzing
            Assert.AreEqual(1, callbackCount); 
            Assert.AreEqual(GameState.None, gameStateChange.previousGameState);
            Assert.AreEqual(GameState.Initializing, gameStateChange.newGameState);
        }
    }
}
