using NUnit.Framework;
using PsycheOpoly.Board;
using PsycheOpoly.Events;
using UnityEngine;
using UnityEngine.TestTools;

namespace PsycheOpoly.Tests
{
    // TODO: Check if this is needed, it seems monolithic and impossible to actually maintain
    public class EventCommunicationTests
    {
        private GameObject root;
        private GameManager gameManager;
        private PlayerManager playerManager;
        private BoardManager boardManager;

        private TurnStartedEventChannel turnStartedChannel;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("TestRoot");
            
            gameManager = root.AddComponent<GameManager>();
            playerManager = root.AddComponent<PlayerManager>();
            boardManager = root.AddComponent<BoardManager>();

            //Added to help with testing setup to prevent null objects 
            var stateCh = ScriptableObject.CreateInstance<GameStateChangedEventChannel>();
            gameManager.gameStateChangedChannel = stateCh;

            turnStartedChannel = ScriptableObject.CreateInstance<TurnStartedEventChannel>();
            gameManager.turnStartedChannel = turnStartedChannel;


            //null ref exceptions were caused originally in this test because the test envrment 
            //wasn't recognizing these objects so i just created mocks essentially for objects where
            //null ref exceptions were occurring -- US-103-hotfix
            gameManager.gameStateChangedChannel = ScriptableObject.CreateInstance<GameStateChangedEventChannel>();
            gameManager.turnStartedChannel = ScriptableObject.CreateInstance<TurnStartedEventChannel>();
            gameManager.playerMovedChannel = ScriptableObject.CreateInstance<PlayerMovedEventChannel>();
            gameManager.initializePlayerCountChannel = ScriptableObject.CreateInstance<IntEventChannel>();
            playerManager.playerAddedEventChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            playerManager.playerRemovedEventChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            playerManager.initializePlayerCountChannel = gameManager.initializePlayerCountChannel;

            boardManager.InitializeBoard(10);
        }

        [TearDown]
        public void TearDown()
        {
            if (turnStartedChannel) Object.DestroyImmediate(turnStartedChannel);
            if (root) Object.DestroyImmediate(root);
        }
        // Task 115: Create a test that will automatically execute one full communication cycle to make sure all scripts can communication between systems.
        // Task 116: Create an Test that verifies event-based communication between files GameEvents, BoardManager, PlayerManager.
        // Task 117: Make sure that all event and player tests run successfully at once.
        [Test]
        public void FullGameCommunicationCycle_Passes()
        {
            turnStartedChannel.Subscribe(tse =>
            {
               Debug.Log($"[Test] TurnStarted: Player ID {tse.playerId}");
            });

            // Task 118: Add assertions or logging that confirms all interfile communication happens in the correct order during the game cycle
            // 1) State should go from None to Initializing
            LogAssert.Expect(LogType.Log, "[GameManager] State: None > Initializing");
            // 2) Intialize() should be called successfully
            LogAssert.Expect(LogType.Log, "Initialize() successfully called - test passed!");
            // 3) State should go from Initializing to WaitingForTurn
            LogAssert.Expect(LogType.Log, "[GameManager] State: Initializing > WaitingForTurn");
            // 4) Just to confirm that the turn started with Player 0
            // Implement once turn start logic is fully implemented and logged: LogAssert.Expect(LogType.Log, "[Test] TurnStarted: Player 0");

            // Note: Start the game with 2 players
            gameManager.StartGame(2);

            // Making sure 2 players exist
            var players = playerManager.GetAllPlayers();
            Assert.AreEqual(2, players.Count, "PlayerManager should create exactly 2 players.");

            // Making sure player 0 exists and is the starting player
            Assert.IsNotNull(players[0], "Player 0 should exist.");
            Assert.AreEqual(0, players[0].GetId(), "Player 0 should have ID = 0.");

            // Testing player 0 move through the event system
            GameEvents.RaisePlayerMoved(0, 1);

            // TODO: Log moves later in BoardManager.
            //LogAssert.Expect(LogType.Log, "[BoardManager] Player 0 moved to 1");

            // Confirm BoardManager updated Player 0 position to 1
            int posPlayer0 = boardManager.GetPlayerPosition(0);
            Assert.AreEqual(1, posPlayer0, "BoardManager should have moved Player 0 by 1 space (to index 1).");

            // TODO: There aren't any checks to check the current player turn, implement later
            // Nor the turn logic in GameManager, implement later.
            // Once implemented we can add: int currentPlayer = gameManager.GetCurrentPlayer();
            gameManager.NextTurn();

            // Testing player 1 move through the event system
            GameEvents.RaisePlayerMoved(1, 2);

            // Confirm BoardManager updated Player 1 position to 2
            int posPlayer1 = boardManager.GetPlayerPosition(1);
            Assert.AreEqual(2, posPlayer1, "BoardManager should have moved Player 0 by 1 space (to index 1).");
        }
    }
}