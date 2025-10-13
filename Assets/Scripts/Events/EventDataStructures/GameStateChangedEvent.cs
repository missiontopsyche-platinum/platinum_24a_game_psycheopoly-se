using UnityEngine;

public class GameStateChangedEvent
{
    /*
     * Player moved event class. Used as a data structure to pass data for the event system.
     * Currently, there are no setter methods, as all data 
     *  should be entered at runtime, and then return on the listening class
     * This can change in a future revision if necessary.
     */

    private GameState currentGameState; //Tracks current gamestate. May be useful later.
    private GameState newGameState;     //Tracks new gamestate.

    public GameStateChangedEvent(GameState currentGameState, GameState newGameState)
    {
        this.currentGameState = currentGameState;
        this.newGameState = newGameState;
    }
}
