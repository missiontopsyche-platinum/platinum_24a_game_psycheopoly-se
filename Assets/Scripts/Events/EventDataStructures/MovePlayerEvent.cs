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
    public int id{ get; private set; }

    /// <summary>
    /// The amount of spaces to move the player
    /// </summary>

    public int spacesToMove { get; private set; }

    /// <summary>
    /// ordered list of board indices the player will traverse. Used by animation systems or to pre-define 
    /// the movement path.
    /// </summary>
    public int[] pathIndices { get; private set; }


    /// <summary>
    /// Creates a <b>READ-ONLY</b> payload of data regarding how to move a player around the board
    /// in the <c>MovePlayerEventChannel</c>
    /// </summary>
    /// <param name="playerId"><b>READ-ONLY:</b> ID of the <c>Player</c></param>
    /// <param name="spacesToMove"><b>READ-ONLY:</b> Amount of spaces to move <c>Player</c></param>
    /// /// <param name="pathIndices">
    public MovePlayerEvent(int playerId, int spacesToMove, int[] pathIndices = null)
    {
        this.id = playerId;
        this.spacesToMove = spacesToMove;
        this.pathIndices = pathIndices ?? new int[0];
    }
}
