using UnityEngine;

/// <summary>
/// <c>GameStateChangedEvent</c> is a payload package of information around the Game State changing.
/// Included is information about the new game state and the old game state.
/// </summary>
public class GameStateChangedEvent
{
    /// <summary>
    /// The previous <c>GameState</c>
    /// </summary>
    public GameState previousGameState { get; private set; }
    /// <summary>
    /// The new (current) <c>GameState</c>
    /// </summary>
    public GameState newGameState { get; private set; }

    /// <summary>
    /// Creates a <c>GameStateChangedEvent</c> populated with <b>read-only</b>
    /// previous and new states of the game.
    /// </summary>
    /// <param name="previousGameState"><b>READ-ONLY:</b> previous <c>GameState</c></param>
    /// <param name="newGameState"><b>READ-ONLY:</b> new (current) <c>GameState</c></param>
    public GameStateChangedEvent(GameState previousGameState, GameState newGameState)
    {
        this.previousGameState = previousGameState;
        this.newGameState = newGameState;
    }
}
