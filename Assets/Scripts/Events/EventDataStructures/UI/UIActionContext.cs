namespace Events.EventDataStructures.UI
{
    /// <summary>
    /// Generic class descriptor for UIActionContexts.
    /// Concrete implementations inherit for this so our events
    /// can make use of polymorphism.
    /// </summary>
    public abstract class UIActionContext { }
    
    //-------------------------------
    
    // !! Definitions for contexts go below.
    //
    // If we decide that this file is growing too large, we can refactor it into separate files.
    // But since we probably only have between 5-7 UI contexts to define, and they're just simple
    // data containers without logic, I can't imagine right now this ballooning too large. 
    
    //-------------------------------

    /// <summary>
    /// UI Action context for purchasing property.
    /// </summary>
    public class PurchaseActionContext : UIActionContext
    {
        public bool Purchased { get; }
        public OwnableSpaceData Property { get; }

        /// <summary>
        /// UI Action context for Property Purchase UI. This contains the information needed to resolve
        /// the UI action for property purchase on the Player Controller end.
        /// </summary>
        /// <param name="purchased"></param>
        /// <param name="property"></param>
        public PurchaseActionContext(bool purchased, OwnableSpaceData property)
        {
            Purchased = purchased;
            Property = property;
        }
    }


    /// <summary>
    /// UI Action context for getting a list of mortageable properties for a player]
    /// Currently not used. May be implemented later if necessary
    /// </summary>
    public class MortagePropertyListContext : UIActionContext
    {
        public bool Player { get; }
    }

    /// <summary>
    /// UI Action context for mortaging a property.
    /// </summary>
    public class MortagePropertyContext : UIActionContext
    {
        public OwnableSpaceData tile { get; }
        public Player player { get; }

        /// <summary>
        /// Contains the player and space data necessary to mortage a property.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        public MortagePropertyContext(Player player, OwnableSpaceData tile) {
            this.tile = data;
            this.player = player;
        }

    }
}