namespace Events.EventDataStructures
{
    public class RentComputedEvent
    {
        public Player Tenant { get; }
        public Player Owner { get; }
        public string TileName { get; }
        public int BaseRent { get; }
        public int FinalRent { get; }

        public RentComputedEvent(Player tenant, Player owner, string tileName, int baseRent, int finalRent)
        {
            Tenant = tenant;
            Owner = owner;
            TileName = tileName;
            BaseRent = baseRent;
            FinalRent = finalRent;
        }
    }
}