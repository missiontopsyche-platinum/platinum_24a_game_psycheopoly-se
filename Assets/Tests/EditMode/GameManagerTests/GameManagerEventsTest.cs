using NUnit.Framework;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerEventsTest : GameManagerTestBase
    {
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
        
            gameManager.gameStateChangedChannel.Subscribe(Listener);

            gameManager.Initialize();

            //this is none > initialzing
            Assert.AreEqual(1, callbackCount); 
            Assert.AreEqual(GameState.None, gameStateChange.previousGameState);
            Assert.AreEqual(GameState.Initializing, gameStateChange.newGameState);
        }
    }
}
