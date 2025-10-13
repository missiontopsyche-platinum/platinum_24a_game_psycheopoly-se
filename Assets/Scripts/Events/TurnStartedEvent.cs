using UnityEngine;

public class TurnStartedEvent
{
    /*
     * Turn started class. Used as a data structure to pass data for the event system.
     * Currently, there are no setter methods, as all data 
     *  should be entered at runtime, and then return on the listening class
     * This can change in a future revision if necessary.
     */

    //members
    private int id; // Tracks player by id.
    private int turnNum; // Tracks turn number. May or may not be useful later on.

    public TurnStartedEvent(int id, int turnNum)
    {
        this.id = id;
        this.turnNum = turnNum;
    }

    public int GetId()
    {
        return this.id;
    }

    public int GetTurnNum()
    {
        return turnNum;
    }
}
