using Events.EventDataStructures;

namespace Managers.PlayerControllers
{
    public class AIPlayerController: PlayerController
    {
        // attributes
        
        // event channels ... I don't think this will need special ones

        /// <summary>
        /// Creates an AI player controller. This needs to be called in conjunction with <c>.Subscribe()</c>
        /// so that event channels are properly routed.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="turnStarted"></param>
        /// <param name="purchaseRequest"></param>
        /// <param name="chargeOwnershipFee"></param>
        /// <param name="passedGoPayment"></param>
        public AIPlayerController(
            Player player,
            TurnStartedEventChannel turnStarted,
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment) 
            : base(player, turnStarted, purchaseRequest, chargeOwnershipFee, passedGoPayment)
        {
            // do AI specific set up here
        }

        public override void Subscribe()
        {
            base.Subscribe();
            
            purchaseOwnableRequestEventChannel.Subscribe(PurchaseRequestDecision);
        }

        private void PurchaseRequestDecision(PurchaseOwnableRequestEvent pore)
        {
            // todo implement
        }
    }
}