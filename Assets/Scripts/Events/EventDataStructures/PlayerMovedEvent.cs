using UnityEngine;

/// <summary>
/// The <c>PlayerMovedEvent</c> contains a <b>READ-ONLY</b> payload of information on player movements used for Event Channels.
/// </summary>
public class PlayerMovedEvent
{
    /// <summary>
    /// The ID of the Player that was moved.
    /// </summary>
    private int id { get; }
    /// <summary>
    /// The previous position of the Player that was moved.
    /// </summary>
    private int previousPosition { get; }
    /// <summary>
    /// The current position of the Player that was moved.
    /// </summary>
    private int newPosition { get; }

    /// <summary>
    /// Creates a <b>READ-ONLY</b> payload of data regarding a Player moving to be used
    /// in the <c>PlayerMovedEventChannel</c>.
    /// </summary>
    /// <param name="id"><b>READ-ONLY:</b> ID of the <c>Player</c></param>
    /// <param name="previousPosition"><b>READ-ONLY:</b> previous <c>Player</c> position</param>
    /// <param name="newPosition"><b>READ-ONLY:</b> new/current <c>Player</c> position</param>
    public PlayerMovedEvent(int id, int previousPosition, int newPosition)
    {
        this.id = id;
        this.previousPosition = previousPosition;
        this.newPosition = newPosition;
    }
}
