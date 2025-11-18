namespace Events.EventDataStructures
{
    public class PayPlayerEvent
    {
        public Player paidPlayer { get; private set; }
        public int amountPaid { get; private set; }

        public PayPlayerEvent(Player paidPlayer, int amountPaid)
        {
            this.paidPlayer = paidPlayer;
            this.amountPaid = amountPaid;
        }
    }
}