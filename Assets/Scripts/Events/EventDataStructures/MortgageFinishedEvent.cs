using UnityEngine;

public class MortgageFinishedEvent
{
    //changed to public fields to be read by listeners
    public Player Player { get; }
    public OwnableSpaceData Tile { get; }

    public MortgageFinishedEvent(Player player, OwnableSpaceData tile)
    {
        Player = player;
        Tile = tile;
    }
}
