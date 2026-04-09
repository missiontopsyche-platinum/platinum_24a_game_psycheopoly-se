namespace Events.EventDataStructures.UI
{
    /// <summary>
    /// Generic class descriptor for UIActivationContexts.
    /// Concrete implementations inherit for this so our events
    /// can make use of polymorphism.
    /// </summary>
    public abstract class UIActivationContext { }

    //-------------------------------
    
    // !! Definitions for contexts go below.
    //
    // If we decide that this file is growing too large, we can refactor it into separate files.
    // But since we probably only have between 5-7 UI contexts to define, and they're just simple
    // data containers without logic, I can't imagine right now this ballooning too large. 
    
    //-------------------------------
    
    /// <summary>
    /// Purchase UI Activation context. Contains the information needed to
    /// dynamically render the Property Purchase UI.
    /// </summary>
    public class PurchaseActivationContext : UIActivationContext
    {
        // This seems to be the minimum viable info needed. We could probably add player information like
        // name and their current money, but that might be in other UI places. Regardless, that would be
        // pretty easy to add later if we decide that's a good QOL feature.
        public OwnableSpaceData Property { get; }
        public int Cost { get; }
        public bool CanAfford { get; }

        /// <summary>
        /// Creates a PurchaseActivationContext to allow the Purchase UI to render dynamically with the
        /// correct information.
        /// </summary>
        /// <param name="property">Property being considered for sale</param>
        /// <param name="cost">Cost of the property</param>
        /// <param name="canAfford">Can the player afford the property?</param>
        public PurchaseActivationContext(OwnableSpaceData property, int cost, bool canAfford)
        {
            Property = property;
            Cost = cost;
            CanAfford = canAfford;
        }
    }

    public class PropertyManagementActivationContext : UIActivationContext
    {
        public Player Player { get; }
        public bool IsDebtResolutionMode { get; }
        public int DebtAmount { get; }

        public PropertyManagementActivationContext(Player player)
        {
            Player = player;
            IsDebtResolutionMode = false;
            DebtAmount = 0;
        }

        public PropertyManagementActivationContext(Player player, bool isDebtResolutionMode, int debtAmount)
        {
            Player = player;
            IsDebtResolutionMode = isDebtResolutionMode;
            DebtAmount = debtAmount;
        }
    }


    /// <summary>
    /// Jail Options UI activation. 
    /// Contains all info to render a Jail Options UI for a player
    /// </summary>
    public class JailActivationContext : UIActivationContext
    {
        public string PlayerName { get; }
        public int JailTurns { get; }
        public int MaxJailTurns { get; }
        public bool CanAffordFine { get; }
        public bool HasGetOutOfJailCard { get; }
        public int FineAmount { get; }

        public JailActivationContext(
            string playerName,
            int jailTurns,
            int maxJailTurns,
            bool canAffordFine,
            bool hasGetOutOfJailCard,
            int fineAmount)
        {
            PlayerName = playerName;
            JailTurns = jailTurns;
            MaxJailTurns = maxJailTurns;
            CanAffordFine = canAffordFine;
            HasGetOutOfJailCard = hasGetOutOfJailCard;
            FineAmount = fineAmount;
        }
    }
}