using UnityEngine;

public class PlayerMovedEvent
{
    /*
     * Player moved event class. Used as a data structure to pass data for the event system.
     * Currently, there are no setter methods, as all data 
     *  should be entered at runtime, and then return on the listening class
     * This can change in a future revision if necessary.
     */

    private int id;                 //Tracks player by id.
    private int currentPosition;    // Tracks current position. May be useful later.
    private int newPosition;        // Tracks new position.

    public PlayerMovedEvent(int id, int currentPosition, int newPosition)
    {
        this.id = id;
        this.currentPosition = currentPosition;
        this.newPosition = newPosition;
    }


    public int GetId()
    {
        return id;
    }

    public int GetCurrentPosition()
    {
        return this.currentPosition;
    }


    public int GetNewPosition()
    {
        return this.newPosition;
    }
}
