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
<<<<<<< HEAD
    public int id{ get; private set; }
=======
    private int id{ get; }
>>>>>>> 6a5dbbc (Task 218 - Create new event channel used to notify BoardManager to move player. This might be scrapped later)

    /// <summary>
    /// The amount of spaces to move the player
    /// </summary>
<<<<<<< HEAD
    public int spacesToMove { get; private set; }
=======
    private int spacesToMove { get; }
>>>>>>> 6a5dbbc (Task 218 - Create new event channel used to notify BoardManager to move player. This might be scrapped later)

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
