namespace Events.EventDataStructures
{
    public class PurchaseUpgradeRequestEvent
    {
        public Player player { get; private set; }
        public PropertySpaceData property { get; private set; }
        public int currentUpgradeLevel { get; private set; }
        public int cost { get; private set; }

        public PurchaseUpgradeRequestEvent(Player player, PropertySpaceData property, int currentUpgradeLevel, int cost)
        {
            this.player = player;
            this.property = property;
            this.currentUpgradeLevel = currentUpgradeLevel;
            this.cost = cost;
        }
    }
}