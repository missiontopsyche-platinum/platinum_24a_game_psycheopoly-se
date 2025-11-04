using UnityEngine;

/// <summary>
/// The <c>TurnStartedEvent</c> contains a <b>READ-ONLY</b> payload of information on
/// new turn information, including turn number and Player ID whose turn it is.
/// </summary>
public class TurnStartedEvent
{
    /// <summary>
    /// The ID of the Player whose turn it currently is.
    /// </summary>
    public int playerId { get; private set; }
    /// <summary>
    /// The current turn number.
    /// </summary>
    public int turnNum { get; private set; }

    /// <summary>
    /// /// Creates a <b>READ-ONLY</b> payload of data regarding a turn starting to be used
    /// in the <c>TurnStartedEventChannel</c>.
    /// </summary>
    /// <param name="playerId"><b>READ-ONLY:</b> ID of the current turns player</param>
    /// <param name="turnNum"><b>READ-ONLY:</b> The current turn number</param>
    public TurnStartedEvent(int playerId, int turnNum)
    {
        this.playerId = playerId;
        this.turnNum = turnNum;
    }
}
