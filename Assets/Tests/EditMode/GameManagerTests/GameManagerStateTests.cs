using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerStateTests : GameManagerTestBase
    {
        // 1) StartGame → Initialize → WaitingForTurn and emits two state-change events
        [Test]
        public void StartGame_initializes_then_enters_WaitingForTurn_and_raises_events()
        {
            var seen = new List<GameStateChangedEvent>();
            gameManager.gameStateChangedChannel.Subscribe(gsc => seen.Add(gsc));

            gameManager.StartGame();

            // Final state
            Assert.AreEqual(GameState.WaitingForTurn, gameManager.gameState, "Should be waiting for turn after StartGame.");

            // Event sequence
            Assert.GreaterOrEqual(seen.Count, 2, "Expected at least two state-change events.");
            // TODO verify this works for GameStateChangedEvent
            Assert.AreEqual((GameState.None, GameState.Initializing), (seen[0].previousGameState, seen[0].newGameState));
            Assert.AreEqual((GameState.Initializing, GameState.WaitingForTurn), (seen[1].previousGameState, seen[1].newGameState));
        }

        // 2) EndGame is legal from WaitingForTurn, and Initialize restarts properly
        [Test]
        public void EndGame_is_legal_and_Initialize_restarts()
        {
            gameManager.StartGame();
            Assert.AreEqual(GameState.WaitingForTurn, gameManager.gameState);

            gameManager.EndGame();
            Assert.AreEqual(GameState.GameOver, gameManager.gameState);

            gameManager.Initialize();
            Assert.AreEqual(GameState.Initializing, gameManager.gameState);
        }

        // 3) StartGame fires turnStartedChannel with the first player payload
        [Test]
        public void StartGame_raises_turnStarted_for_first_player()
        {
            TurnStartedEvent receivedTse = null;
            gameManager.turnStartedChannel.Subscribe(p => receivedTse = p);

            gameManager.StartGame();
            gameManager.CompleteGameInit();

            Assert.IsNotNull(receivedTse, "turnStartedChannel should fire with a Player payload.");
            Assert.AreEqual(0, receivedTse.playerId);
        }
    }
}
