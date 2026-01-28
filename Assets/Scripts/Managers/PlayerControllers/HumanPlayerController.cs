using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;

namespace Managers.PlayerControllers
{
    public class HumanPlayerController : PlayerController
    {
        // attributes
        
        // event channels
        private UIActivationEventChannel uiActivationEventChannel;
        private UIActionEventChannel uiActionEventChannel;
        
        // I need to figure out the architecture for UI events that the human controller will make use of
        // before I get too deep into this one- so I'll shelve it for a bit until I can work that out with
        // the UI team.

        /// <summary>
        /// Creates Human Player controller. Once called, it must have <c>Subscribe()</c> called on it to ensure
        /// all event channels are properly subscribed.
        /// </summary>
        /// <param name="player">Player ScriptableObject the controller is responsible for</param>
        /// <param name="turnStarted">TurnStartedEventChannel</param>
        /// <param name="purchaseRequest">PurchaseOwnableRequestEventChannel</param>
        /// <param name="chargeOwnershipFee">ChargeOwnershipFeeEventChannel</param>
        /// <param name="passedGoPayment">PayPlayerEventChannel for passing Go</param>
        /// <param name="uiActivation">UI Activation Event Channel</param>
        /// <param name="uiAction">UI Action Event Channel</param>
        public HumanPlayerController(
            Player player,
            TurnStartedEventChannel turnStarted,
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment,
            UIActivationEventChannel uiActivation,
            UIActionEventChannel uiAction) 
            : base(player, turnStarted, purchaseRequest, chargeOwnershipFee, passedGoPayment)
        {
            // human controller specific setup goes here
            uiActivationEventChannel = uiActivation;
            uiActionEventChannel = uiAction;
        }

        ~HumanPlayerController()
        {
            Unsubscribe();
        }

        public override void Subscribe()
        {
            base.Subscribe();
            uiActionEventChannel.Subscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel.Subscribe(HandlePurchaseOwnableEvent);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            uiActionEventChannel.Unsubscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel.Unsubscribe(HandlePurchaseOwnableEvent);
        }

        private void HandlePurchaseOwnableEvent(PurchaseOwnableRequestEvent pore)
        {
            if (!isMyTurn) return;
            
            uiActivationEventChannel.RaiseEvent(
                new UIActivationEvent(
                    UIType.PropertyPurchase, 
                    new PurchaseActivationContext(
                        pore.requestedSpace,
                        pore.cost,
                        controlledPlayer.GetMoney() >= pore.cost)));
        }

        private void HandleChargeOwnership(ChargeOwnershipFeeEvent cofe)
        {
            if (!isMyTurn) return;
            
            // activate Rent notification UI
            // call Player method for charging rent
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            if (!isMyTurn) return;
            
            // handle passing go
            // call player method for getting paid for passing go
        }

        private void HandleUIAction(UIActionEvent uiae)
        {
            if (!isMyTurn) return;
            
            switch (uiae.UIType)
            {
                // We should probably have some guard clauses to make sure
                // context is the correct type before casting like this.
                case UIType.PropertyPurchase:
                    ResolvePropertyPurchase((PurchaseActionContext)uiae.Context);
                    break;
                // expand with more UITypes as they're implemented
                default:
                    break;
            }
        }

        private void ResolvePropertyPurchase(PurchaseActionContext pac)
        {
            // call Player methods to resolve purchase
        }
    }
}