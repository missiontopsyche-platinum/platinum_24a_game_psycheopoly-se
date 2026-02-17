using UnityEngine;

public class MortageFinishedEvent
{
    Player player { get; }
    OwnableSpaceData tile { get; }

    public MortageFinishedEvent(Player player, OwnableSpaceData tile)
    {
        this.player = player;
        this.tile = tile;
    }
}
