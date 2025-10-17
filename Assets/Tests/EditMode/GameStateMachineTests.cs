using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class GameManagerStateTests
{
    // Helper: make a fresh GameManager with dependencies & injectable channels
    private GameManager MakeGM(
        out EventChannel<GameStateChangedEvent> stateChan,
        out PlayerEventChannel turnChan)
    {
        var go = new GameObject("GM");
        var gm = go.AddComponent<GameManager>();

        // Hard dependency
        gm.playerManager = new GameObject("PM").AddComponent<PlayerManager>();

        // Create channels and inject them via reflection (they're [SerializeField])
        stateChan = ScriptableObject.CreateInstance<EventChannel<GameStateChangedEvent>>();
        turnChan  = ScriptableObject.CreateInstance<PlayerEventChannel>();

        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        gm.GetType().GetField("gameStateChangeChannel", flags)
          ?.SetValue(gm, stateChan);

        gm.GetType().GetField("turnStartedChannel", flags)
          ?.SetValue(gm, turnChan);

        return gm;
    }

    // 1) StartGame → Initialize → WaitingForTurn and emits two state-change events
    [Test]
    public void StartGame_initializes_then_enters_WaitingForTurn_and_raises_events()
    {
        var gm = MakeGM(out var stateChan, out _);

        var seen = new List<(GameState fromS, GameState toS)>();
        stateChan.Subscribe(gsc => seen.Add((gsc.previousGameState, gsc.newGameState)));

        gm.StartGame(2);

        // Final state
        Assert.AreEqual(GameState.WaitingForTurn, gm.gameState, "Should be waiting for turn after StartGame.");

        // Event sequence
        Assert.GreaterOrEqual(seen.Count, 2, "Expected at least two state-change events.");
        Assert.AreEqual((GameState.None,         GameState.Initializing),   seen[0]);
        Assert.AreEqual((GameState.Initializing, GameState.WaitingForTurn), seen[1]);
    }

    // 2) EndGame is legal from WaitingForTurn, and Initialize restarts properly
    [Test]
    public void EndGame_is_legal_and_Initialize_restarts()
    {
        var gm = MakeGM(out _, out _);

        gm.StartGame(2);
        Assert.AreEqual(GameState.WaitingForTurn, gm.gameState);

        gm.EndGame();
        Assert.AreEqual(GameState.GameOver, gm.gameState);

        gm.Initialize();
        Assert.AreEqual(GameState.Initializing, gm.gameState);
    }

    // 3) StartGame fires turnStartedChannel with the first player payload
    [Test]
    public void StartGame_raises_turnStarted_for_first_player()
    {
        var gm = MakeGM(out _, out var turnChan);

        Player received = null;
        turnChan.Subscribe(p => received = p);

        gm.StartGame(2);

        Assert.IsNotNull(received, "turnStartedChannel should fire with a Player payload.");
        Assert.AreEqual("Player 1", received.GetPName());
    }

    // 4) Invalid player counts are rejected and do not change state
    [Test]
    public void StartGame_with_invalid_player_count_does_not_change_state()
    {
        var gm = MakeGM(out _, out _);

        // start in None; invalid counts (e.g., 1 or 5) should not transition
        gm.StartGame(1);
        Assert.AreEqual(GameState.None, gm.gameState);

        gm.StartGame(5);
        Assert.AreEqual(GameState.None, gm.gameState);
    }
}
