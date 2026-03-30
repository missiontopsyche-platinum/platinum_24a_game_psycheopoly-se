using Data;

namespace Events.EventDataStructures
{
    public class UpgradeRequestEvent
    {
        public Player player { get; }
        public PropertySpaceData property { get; }

        public UpgradeRequestEvent(Player player, PropertySpaceData property)
        {
            this.player = player;
            this.property = property;
        }
    }
}