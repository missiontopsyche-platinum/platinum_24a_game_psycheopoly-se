using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTurnRotationTests : GameManagerTestBase
    {
    
        private int lastReceivedPlayer = -1;
    
        [Test]
        public void NextTurn_LoopBackWithTwoPlayers()
        {
            int callbackCount = 0;
        
            void Listener(TurnStartedEvent tse)
            {
                callbackCount++;
                lastReceivedPlayer = tse.playerId;
            }
            gameManager.turnStartedChannel.Subscribe(Listener);
        
            gameManager.SetUpGame(2);
            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        }
    
        [Test]
        public void NextTurn_LoopBackWithThreePlayers()
        {
            int callbackCount = 0;
        
            void Listener(TurnStartedEvent tse)
            {
                callbackCount++;
                lastReceivedPlayer = tse.playerId;
            }
            gameManager.turnStartedChannel.Subscribe(Listener);
        
            gameManager.SetUpGame(3);
            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(2, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(4, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        }
    
        [Test]
        public void NextTurn_LoopBackWithFourPlayers()
        {
            int callbackCount = 0;
        
            void Listener(TurnStartedEvent tse)
            {
                callbackCount++;
                lastReceivedPlayer = tse.playerId;
            }
            gameManager.turnStartedChannel.Subscribe(Listener);
        
            gameManager.SetUpGame(4);
            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(2, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(4, callbackCount);
            Assert.AreEqual(3, lastReceivedPlayer);
        
            gameManager.NextTurn();
            Assert.AreEqual(5, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        }

        [Test]
        public void SetUpGame_LogsErrorWhenInvalidPlayerCount()
        {
            gameManager.SetUpGame(1);
            LogAssert.Expect("Test [Level: Error] " +
                "[Category: Gameplay] " +
                "[Event Name: GameManager.SetUpGame] " +
                "[Message: Invalid player count, must be between 2 and 4.]");
        
            gameManager.SetUpGame(5);
            LogAssert.Expect("Test [Level: Error] " +
                "[Category: Gameplay] " +
                "[Event Name: GameManager.SetUpGame] " +
                "[Message: Invalid player count, must be between 2 and 4.]");
        }
    }
}