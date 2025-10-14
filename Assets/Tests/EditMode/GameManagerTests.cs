using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode
{
    public class GameManagerTests
    {
        private PlayerEventChannel turnStartedChannel;
        private int callbackCount;
        private Player lastReceivedPlayer;

        private GameObject parentObject;
        private GameManager gameManager;
        private GameStateChangedEventChannel gameStateEventChannel;
        private PlayerManager playerManager;
        private PlayerEventChannel playerEventChannel;

        [SetUp]
        public void SetUp()
        {
            parentObject = new GameObject();
            gameManager = parentObject.AddComponent<GameManager>();
            turnStartedChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            gameManager.turnStartedChannel = turnStartedChannel;
            gameStateEventChannel = ScriptableObject.CreateInstance<GameStateChangedEventChannel>();
            gameManager.gameStateChangedChannel = gameStateEventChannel;
            playerManager = parentObject.AddComponent<PlayerManager>();
            gameManager.playerManager = playerManager;
            playerEventChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            playerManager.playerAddedEventChannel = playerEventChannel;

            callbackCount = 0;
            lastReceivedPlayer = null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(parentObject);
        }

        [Test]
        public void NextTurn_LoopBackWithTwoPlayers()
        {
            void Listener(Player player)
            {
                callbackCount++;
                lastReceivedPlayer = player;
            }
            turnStartedChannel.Subscribe(Listener);
            
            gameManager.SetUpGame(2);
            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer.GetId());
        }
        
        [Test]
        public void NextTurn_LoopBackWithThreePlayers()
        {
            void Listener(Player player)
            {
                callbackCount++;
                lastReceivedPlayer = player;
            }
            turnStartedChannel.Subscribe(Listener);
            
            gameManager.SetUpGame(3);
            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(2, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(4, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer.GetId());
        }
        
        [Test]
        public void NextTurn_LoopBackWithFourPlayers()
        {
            void Listener(Player player)
            {
                callbackCount++;
                lastReceivedPlayer = player;
            }
            turnStartedChannel.Subscribe(Listener);
            
            gameManager.SetUpGame(4);
            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(2, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(4, callbackCount);
            Assert.AreEqual(3, lastReceivedPlayer.GetId());
            
            gameManager.NextTurn();
            Assert.AreEqual(5, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer.GetId());
        }

        [Test]
        public void SetUpGame_LogsErrorWhenInvalidPlayerCount()
        {
            gameManager.SetUpGame(1);
            LogAssert.Expect("Invalid player count, must be between 2 and 4.");
            
            gameManager.SetUpGame(5);
            LogAssert.Expect("Invalid player count, must be between 2 and 4.");
        }
    }
}