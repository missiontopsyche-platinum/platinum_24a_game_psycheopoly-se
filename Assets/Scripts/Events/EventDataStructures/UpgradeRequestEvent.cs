using UnityEngine;

namespace Events.EventDataStructures
{
    public struct UpgradeRequestEvent
    {
        public Player Player;
        public PropertySpaceData Tile;

        public UpgradeRequestEvent(Player player, PropertySpaceData tile)
        {
            Player = player;
            Tile = tile;
        }
    }
}