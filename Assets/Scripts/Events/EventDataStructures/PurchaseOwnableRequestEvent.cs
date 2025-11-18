namespace Events.EventDataStructures
{
    public class PurchaseOwnableRequestEvent
    {
        public Player requestedPlayer { get; private set; }
        public OwnableSpaceData requestedSpace { get; private set; }
        public int cost { get; private set; }

        public PurchaseOwnableRequestEvent(Player requestedPlayer, OwnableSpaceData requestedSpace, int cost)
        {
            this.requestedPlayer = requestedPlayer;
            this.requestedSpace = requestedSpace;
            this.cost = cost;
        }
    }
}