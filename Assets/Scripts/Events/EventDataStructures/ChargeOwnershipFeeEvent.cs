namespace Events.EventDataStructures
{
    public class ChargeOwnershipFeeEvent
    {
        public Player fromPlayer { get; private set; }
        public Player toPlayer { get; private set; }
        public int amount { get; private set; }
        public OwnableSpaceData sourceSpace { get; private set; }

        public ChargeOwnershipFeeEvent(Player fromPlayer, Player toPlayer, int amount, OwnableSpaceData sourceSpace)
        {
            this.fromPlayer = fromPlayer;
            this.toPlayer = toPlayer;
            this.amount = amount;
            this.sourceSpace = sourceSpace;
        }
    }
}