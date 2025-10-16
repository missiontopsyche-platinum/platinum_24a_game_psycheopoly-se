using NUnit.Framework;
using System.Collections.Generic;

public class GameManagerStateTests : GameManagerTestBase
{
    // 1) StartGame → Initialize → WaitingForTurn and emits two state-change events
    [Test]
    public void StartGame_initializes_then_enters_WaitingForTurn_and_raises_events()
    {
        var seen = new List<GameStateChangedEvent>();
        gameStateEventChannel.Subscribe(gsc => seen.Add(gsc));

        gameManager.StartGame(2);

        // Final state
        Assert.AreEqual(GameState.WaitingForTurn, gameManager.gameState, "Should be waiting for turn after StartGame.");

        // Event sequence
        Assert.GreaterOrEqual(seen.Count, 2, "Expected at least two state-change events.");
        // TODO verify this works for GameStateChangedEvent
        Assert.AreEqual((GameState.None,         GameState.Initializing),   seen[0]);
        Assert.AreEqual((GameState.Initializing, GameState.WaitingForTurn), seen[1]);
    }

    // 2) EndGame is legal from WaitingForTurn, and Initialize restarts properly
    [Test]
    public void EndGame_is_legal_and_Initialize_restarts()
    {
        gameManager.StartGame(2);
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
        turnStartedChannel.Subscribe(p => receivedTse = p);

        gameManager.StartGame(2);

        Assert.IsNotNull(receivedTse, "turnStartedChannel should fire with a Player payload.");
        Assert.AreEqual(0, receivedTse.playerId);
    }

    // 4) Invalid player counts are rejected and do not change state
    [Test]
    public void StartGame_with_invalid_player_count_does_not_change_state()
    { 
        // start in None; invalid counts (e.g., 1 or 5) should not transition
        gameManager.StartGame(1);
        Assert.AreEqual(GameState.None, gameManager.gameState);

        gameManager.StartGame(5);
        Assert.AreEqual(GameState.None, gameManager.gameState);
    }
}
