using Events.EventDataStructures;
using Logging;
using UnityEngine;

namespace Managers.PlayerControllers
{
    public class PlayerController
    {
        // fields
        protected readonly Player controlledPlayer;
        protected bool isMyTurn = false;

        /// <summary>
        /// Gets the Player ScriptableObject
        /// </summary>
        public Player GetControlledPlayer() => controlledPlayer;
        
        // event channels || may need to add more as requirements change. Potentially have all channels in all subclasses.
        private TurnStartedEventChannel turnStartedEventChannel;
        protected PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
        protected ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;
        protected PayPlayerEventChannel passedGoPaymentChannel;
        protected CardDrawnEventChannel cardDrawnEventChannel;

        // constructor, needs to be called in future subclass constructors with `super(args)`
        public PlayerController(
            Player player, 
            TurnStartedEventChannel turnStarted, 
            PurchaseOwnableRequestEventChannel purchaseRequest, 
            ChargeOwnershipFeeEventChannel chargeOwnershipFee, 
            PayPlayerEventChannel passedGoPayment)
        {
            controlledPlayer = player ?? throw new System.ArgumentNullException(nameof(player));
            turnStartedEventChannel = turnStarted ?? throw new System.ArgumentNullException(nameof(turnStarted));
            purchaseOwnableRequestEventChannel =
                purchaseRequest ?? throw new System.ArgumentNullException(nameof(purchaseRequest));
            chargeOwnershipFeeEventChannel =
                chargeOwnershipFee ?? throw new System.ArgumentNullException(nameof(chargeOwnershipFee));
            passedGoPaymentChannel = passedGoPayment ?? throw new System.ArgumentNullException(nameof(passedGoPayment));
        }
        
        // general event handling

        /// <summary>
        /// Subscribe to basic game event channels. This should be extended in subclasses to capture
        /// the distinct methods and behavior events, by calling <c>base.Subscribe()</c>.
        /// </summary>
        public virtual void Subscribe()
        {
            turnStartedEventChannel?.Subscribe(CatchTurnStartedEvent);
        }

        /// <summary>
        /// Unsubscribe from all game event channels. This should be extended in subclasses to account for
        /// subclass-specific event channels and called with <c>base.Unsubscribe()</c>.
        /// </summary>
        public virtual void Unsubscribe()
        {
            turnStartedEventChannel?.Unsubscribe(CatchTurnStartedEvent);
        }
        
        /// <summary>
        /// Catches the TurnStartedEvent, and evaluates if it is this Player's turn or not, and sets the
        /// boolean state accordingly (<c>isMyTurn</c>).
        /// </summary>
        /// <param name="tse">Turn Started Event, containing payload information about the new turn.</param>
        protected virtual void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            isMyTurn = tse.playerId == controlledPlayer.GetId();
            
            if (isMyTurn)
                Logging.Logger.Info("PlayerController.CatchTurnStartedEvent",
                    $"Player {controlledPlayer.GetId()} turn started.",
                    LogCategory.Gameplay, this);
        }

       
    }
}