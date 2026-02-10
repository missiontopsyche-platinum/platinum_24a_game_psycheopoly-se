using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using Logging;
using UnityEngine;
using Logger = Logging.Logger;

namespace Managers.PlayerControllers
{
    public class HumanPlayerController : PlayerController
    {
        // attributes
        
        // event channels
        private UIActivationEventChannel uiActivationEventChannel;
        private UIActionEventChannel uiActionEventChannel;

        private MortgageFinishedEventChannel mortgageFinishedEventChannel;

        // event channel for bankruptcy
        
        [SerializeField] public IntEventChannel bankruptPlayerEventChannel;
        private MortgageFinishedEventChannel mortageFinishedEventChannel;
        private MortgageFinishedEventChannel mortgageFinishedEventChannel;
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
            UIActionEventChannel uiAction,
            MortgageFinishedEventChannel mortgageFinished) 
            : base(player, turnStarted, purchaseRequest, chargeOwnershipFee, passedGoPayment)
        {
            // human controller specific setup goes here
            uiActivationEventChannel = uiActivation;
            uiActionEventChannel = uiAction;
            mortgageFinishedEventChannel = mortgageFinished;
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
                        controlledPlayer.CanAfford(pore.requestedSpace.buyPrice)))); 

        }

        private void HandleChargeOwnership(ChargeOwnershipFeeEvent cofe)
        {
            if (!isMyTurn) return;
            
            // activate Rent notification UI

            // call Player method for charging rent
            // Flow: Check if player has money -> If yes, try spend, return proper response via event channel
            // If no -> Check for bankrupcy - If bankrupt, notify UI, call proper method on GameManger, call ClearOwnership
            if (!controlledPlayer.CanAfford(cofe.amount))
            {
                if (controlledPlayer.IsBankrupt(cofe.amount))
                {
                    // TODO: Call event channel for UI
                    controlledPlayer.ClearOwnership();
                    // Need to check with Hank to verify GameManager linkage. But currently no link, therefore we will create a "BankruptPlayer" event channel to fire.
                    // Will return an int, only providing the player ID which SHOULD be the turn order number.
                    // This will need to be listend to by the GameManager to remove the player from the order.
                    bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                }
            }
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            if (!isMyTurn) return;
            
            // handle passing go
            // call player method for getting paid for passing go
        }

        private void ResolveMortgageProperty(MortgagePropertyContext context)
        {
            if (!isMyTurn) return;

            if(controlledPlayer.MortgageProperty(context.tile))
            {
                mortgageFinishedEventChannel?.RaiseEvent(new MortgageFinishedEvent(
                    this.controlledPlayer,
                    context.tile));
            }

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
                case UIType.MortgagePropertySelected:
                    if(uiae.Context is MortgagePropertyContext mortgageContext)
                        ResolveMortgageProperty(mortgageContext);
                    else
                        Logger.Error("HumanPlayerController.HandleUIAction",
                            $"Expected MortageActionContext but got {uiae.Context?.GetType().Name}",
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
                controlledPlayer.ExecutePurchase(pac.Property, pac.Property.buyPrice);
                Logger.Debug("HumanPlayerController.ResolvePropertyPurchase",
                    $"{controlledPlayer.GetPName()} has executed purchase on ${pac.Property.name}",
                    LogCategory.Gameplay); 
                //Fire
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