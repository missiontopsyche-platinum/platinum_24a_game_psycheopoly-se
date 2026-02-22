namespace AIBehavior
{
    /// <summary>
    /// Mortgage Action command. Used to communicate to PlayerControllers what to do with properties
    /// that need to be mortgaged/unmortgaged, or have upgrades sold off. Used extensively in 
    /// </summary>
    public class MortgageAction
    {
        public OwnableSpaceData ownableSpace { get; }
        public MortgageActionType actionType { get; }
        public int expectedCashGain { get; }

        public MortgageAction(OwnableSpaceData ownableSpace, MortgageActionType actionType, int expectedCashGain)
        {
            this.ownableSpace = ownableSpace;
            this.actionType = actionType;
            this.expectedCashGain = expectedCashGain;
        }
    }

    public enum MortgageActionType
    {
        Mortgage,
        Unmortgage,
        SellDataPoint,
        SellDiscovery
    }
}