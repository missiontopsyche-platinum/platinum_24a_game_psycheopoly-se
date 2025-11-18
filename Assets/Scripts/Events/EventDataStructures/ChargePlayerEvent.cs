namespace Events.EventDataStructures
{
    public class ChargePlayerEvent
    {
        public Player chargedPlayer { get; private set; }
        public int chargeAmount { get; private set; }

        public ChargePlayerEvent(Player chargedPlayer, int chargeAmount)
        {
            this.chargedPlayer = chargedPlayer;
            this.chargeAmount = chargeAmount;
        }
    }
}