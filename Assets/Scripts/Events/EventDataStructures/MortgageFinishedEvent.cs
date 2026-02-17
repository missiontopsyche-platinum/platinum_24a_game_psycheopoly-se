using UnityEngine;

public class MortgageFinishedEvent
{
    Player player { get; }
    OwnableSpaceData tile { get; }

    public MortgageFinishedEvent(Player player, OwnableSpaceData tile)
    {
        this.player = player;
        this.tile = tile;
    }
}
