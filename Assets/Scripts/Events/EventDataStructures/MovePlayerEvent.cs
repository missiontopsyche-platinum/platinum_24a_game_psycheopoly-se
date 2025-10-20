using UnityEngine;

/// <summary>
/// The <c>MovePlayerEvent</c> contains a <b>READ-ONLY</b> payload of information for passing information about how
/// to move a player used for Event Channels
/// </summary>
public class MovePlayerEvent
{
    /// <summary>
    /// The id of the Player to move
    /// </summary>
    private int id{ get; }

    /// <summary>
    /// The amount of spaces to move the player
    /// </summary>
    private int spacesToMove { get; }

    /// <summary>
    /// Creates a <b>READ-ONLY</b> payload of data regarding how to move a player around the board
    /// in the <c>MovePlayerEventChannel</c>
    /// </summary>
    /// <param name="playerId"><b>READ-ONLY:</b> ID of the <c>Player</c></param>
    /// <param name="spacesToMove"><b>READ-ONLY:</b> Amount of spaces to move <c>Player</c></param>
    public MovePlayerEvent(int playerId, int spacesToMove)
    {
        this.id = playerId;
        this.spacesToMove = spacesToMove;
    }
}
