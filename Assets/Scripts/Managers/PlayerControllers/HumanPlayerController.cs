using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using Logging;

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
            uiActionEventChannel?.Subscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel?.Subscribe(HandlePurchaseOwnableEvent);
            chargeOwnershipFeeEventChannel?.Subscribe(HandleChargeOwnership);
            passedGoPaymentChannel?.Subscribe(HandlePassedGo);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            uiActionEventChannel?.Unsubscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel?.Unsubscribe(HandlePurchaseOwnableEvent);
            chargeOwnershipFeeEventChannel?.Unsubscribe(HandleChargeOwnership);
            passedGoPaymentChannel?.Unsubscribe(HandlePassedGo);
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
                        // controlledPlayer.CanAfford(pore.requestedSpace)))); // when the method is added to Player, we can uncomment this.
                        controlledPlayer.GetMoney() >= pore.cost))); // remove this when Player has CanAfford method
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
                case UIType.PropertyPurchase:
                    if(uiae.Context is PurchaseActionContext purchaseContext)
                        ResolvePropertyPurchase(purchaseContext);
                    else
                        Logger.Error("HumanPlayerController.HandleUIAction",
                            $"Expected PurchaseActionContext but got {uiae.Context?.GetType().Name}",
                            LogCategory.UI);
                    break;
                // expand with more UITypes as they're implemented
                default:
                    Logger.Debug("HumanPlayerController.HandleUIAction",
                        $"Unhandled UI Type: ${uiae.UIType}",
                        LogCategory.UI);
                    break;
            }
        }

        private void ResolvePropertyPurchase(PurchaseActionContext pac)
        {
            // uncomment these when the methods are implemented
            if (pac.Purchased)
            {
                // controlledPlayer.ExecutePurchase(pac.Property);
                Logger.Debug("HumanPlayerController.ResolvePropertyPurchase",
                    $"{controlledPlayer.GetPName()} has executed purchase on ${pac.Property.name}",
                    LogCategory.Gameplay);
            }
            else
            {
                // controlledPlayer.DeclinePurchase(pac.Property);
                Logger.Debug("HumanPlayerController.ResolvePropertyPurchase",
                    $"{controlledPlayer.GetPName()} has declined purchase on ${pac.Property.name}",
                    LogCategory.Gameplay);
            }
        }
    }
}