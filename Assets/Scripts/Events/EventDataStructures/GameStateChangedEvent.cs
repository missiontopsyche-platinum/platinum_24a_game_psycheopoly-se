using UnityEngine;

public class GameStateChangedEvent
{
    /*
     * Player moved event class. Used as a data structure to pass data for the event system.
     * Currently, there are no setter methods, as all data 
     *  should be entered at runtime, and then return on the listening class
     * This can change in a future revision if necessary.
     */

    public GameState previousGameState { get; private set; } //Tracks current gamestate. May be useful later.
    public GameState newGameState { get; private set; }     //Tracks new gamestate.

    public GameStateChangedEvent(GameState previousGameState, GameState newGameState)
    {
        this.previousGameState = previousGameState;
        this.newGameState = newGameState;
    }
}
