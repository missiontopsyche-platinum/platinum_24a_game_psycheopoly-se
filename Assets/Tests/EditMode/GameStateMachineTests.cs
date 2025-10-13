using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GameManagerStateTests
{
    //Creates GameManager
    private GameManager MakeGM()
    {
        var go = new GameObject("GM");
        var gm = go.AddComponent<GameManager>();

        //PlayerManager dependency
        gm.playerManager = new GameObject("PM").AddComponent<PlayerManager>();

        var turnChan = ScriptableObject.CreateInstance<PlayerEventChannel>();
        gm.GetType().GetField("turnStartedChannel",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Public)
          ?.SetValue(gm, turnChan);

        return gm;
    }

    //Checks if game enters proper state after startup
    [Test]
    public void StartGame_initializes_then_enters_WaitingForTurn_and_raises_event()
    {
        var gm = MakeGM();

        var seen = new List<(GameState oldS, GameState newS)>();
        gm.GameStateChanged += (o, n) => seen.Add((o, n));

        gm.StartGame(2);

        //Checks final state
        Assert.AreEqual(GameState.WaitingForTurn, gm.State);

        Assert.GreaterOrEqual(seen.Count, 2);
        Assert.AreEqual(GameState.None,           seen[0].oldS);
        Assert.AreEqual(GameState.Initializing,   seen[0].newS);
        Assert.AreEqual(GameState.Initializing,   seen[1].oldS);
        Assert.AreEqual(GameState.WaitingForTurn, seen[1].newS);
    }

    //Checks if transitions are properly guarded
    [Test]
    public void Guarded_transitions_block_illegal_moves_and_allow_legal_path()
    {
        var gm = MakeGM();

        Assert.IsFalse(gm.TryChangeState(GameState.PlayerTurn));
        Assert.AreEqual(GameState.None, gm.State);

        Assert.IsTrue(gm.TryChangeState(GameState.Initializing));
        Assert.IsTrue(gm.TryChangeState(GameState.WaitingForTurn));
        Assert.AreEqual(GameState.WaitingForTurn, gm.State);

        Assert.IsFalse(gm.TryChangeState(GameState.BotTurn));
        Assert.AreEqual(GameState.WaitingForTurn, gm.State);

        Assert.IsTrue(gm.TryChangeState(GameState.PlayerTurn));
        Assert.AreEqual(GameState.PlayerTurn, gm.State);
    }

    //Checks if endgame state is allowed and if game properly restarts
    [Test]
    public void EndGame_is_legal_and_restart_allowed()
    {
        var gm = MakeGM();

        gm.StartGame(2);
        Assert.AreEqual(GameState.WaitingForTurn, gm.State);

        gm.EndGame();
        Assert.AreEqual(GameState.GameOver, gm.State);

        gm.Initialize(); 
        Assert.AreEqual(GameState.Initializing, gm.State);
    }

    //Checks if start game raises proper event for first player
    [Test]
    public void StartGame_raises_turnStartedChannel_with_first_player()
    {
        var gm = MakeGM();

        var chan = (PlayerEventChannel) gm.GetType().GetField("turnStartedChannel",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Public)?.GetValue(gm);

        Player got = null;
        chan.Subscribe(p => got = p);

        gm.StartGame(2);

        Assert.IsNotNull(got);
        Assert.AreEqual("Player 1", got.GetPName());
    }
}
