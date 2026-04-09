using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Managers.TurnFlow;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using UnityEngine;
using Logging;
using Logger = Logging.Logger;


namespace Managers.PlayerControllers
{
    public class HumanPlayerController : PlayerController
    {
        // attributes

        // event channels

        

        // event channel for bankruptcy
        [SerializeField] public IntEventChannel bankruptPlayerEventChannel;

        private readonly UIActivationEventChannel uiActivationEventChannel;
        private readonly UIActionEventChannel uiActionEventChannel;
        private readonly MortgageFinishedEventChannel mortgageFinishedEventChannel;
        private readonly BooleanEventChannel diceRollPannelEventChannel;


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
        /// <param name="mortgageFinished">Mortgage Finished Event Channel</param>
        public HumanPlayerController(
            Player player,
            TurnStartedEventChannel turnStarted,
            BooleanEventChannel turnEnded,
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment,
            UIActivationEventChannel uiActivation,
            UIActionEventChannel uiAction,
            MortgageFinishedEventChannel mortgageFinished,
            UpgradeRequestEventChannel upgradeRequest,
            IntEventChannel bankruptPlayer,
            TurnActionRequestEventChannel turnActionRequest,
            TurnActionResultEventChannel turnActionResult,
            JailStateChangedEventChannel jailStateChanged,
            BooleanEventChannel diceRollPannel,
            ChargePlayerEventChannel chargePlayer) 
            : base(player, turnStarted, turnEnded, purchaseRequest, chargeOwnershipFee, passedGoPayment, upgradeRequest, turnActionRequest, turnActionResult, bankruptPlayer, jailStateChanged, chargePlayer)
        {
            // human controller specific setup goes here
            uiActivationEventChannel = uiActivation;
            uiActionEventChannel = uiAction;
            mortgageFinishedEventChannel = mortgageFinished;
            diceRollPannelEventChannel = diceRollPannel;
            bankruptPlayerEventChannel = bankruptPlayer;
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
            turnEndedEventChannel?.Subscribe(OnTurnEnded);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            uiActionEventChannel?.Unsubscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel?.Unsubscribe(HandlePurchaseOwnableEvent);
            chargeOwnershipFeeEventChannel?.Unsubscribe(HandleChargeOwnership);
            passedGoPaymentChannel?.Unsubscribe(HandlePassedGo);
            turnEndedEventChannel?.Unsubscribe(OnTurnEnded);
        }

        private void HandlePurchaseOwnableEvent(PurchaseOwnableRequestEvent pore)
        {
            if (!isMyTurn) return;
            // Note: Follow this pattern for any event that requires player input.
            RequestTurnAction(
                TurnActionType.BuyProperty,
                onAllowed: () =>
                {
                    uiActivationEventChannel?.RaiseEvent(
                        new UIActivationEvent(
                            UIType.PropertyPurchase,
                            new PurchaseActivationContext(
                                pore.requestedSpace,
                                pore.cost,
                                controlledPlayer.CanAfford(pore.requestedSpace.buyPrice))));
                },
                onDenied: () =>
                {
                    Logger.Debug("HumanPlayerController.HandlePurchaseOwnableEvent",
                        "Purchase UI blocked by TurnFlow.",
                        LogCategory.UI);
                });
        }

        private void HandleUpgradeEvent(PropertySpaceData property)
        {
            if (!isMyTurn || property == null) return;
            // Note: Follow this pattern for any event that requires player input.
            RequestTurnAction(
                TurnActionType.ModifyProperty,
                onAllowed: () =>
                {
                    upgradeRequestEventChannel?.RaiseEvent(
                        new UpgradeRequestEvent(controlledPlayer, property));

                    Logger.Debug("HumanPlayerController.HandleUpgradeEvent",
                        $"Raised upgrade request for {property.spaceName}.",
                        LogCategory.UI);
                },
                onDenied: () =>
                {
                    Logger.Debug("HumanPlayerController.HandleUpgradeEvent",
                        "Purchase UI blocked by TurnFlow.",
                        LogCategory.UI);
                });
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
                    // TODO: Call event channel for UI to notify of bankruptcy
                    
                    bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                }

                //TODO: for the UI for property management. There needs to be a check to ensure the player CANNOT close the screen once opened until they finish
            }

            controlledPlayer.TrySpend(cofe.amount);
            cofe.toPlayer.AddMoney(cofe.amount);
            RequestResolutionComplete();
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            if (!isMyTurn) return;

            // handle passing go
            // call player method for getting paid for passing go

            RequestResolutionComplete();
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

            RequestResolutionComplete();
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
                case UIType.PropertyUpgradeSelected:
                     if (uiae.Context is PropertyUpgradeContext upgradeContext)
                         HandleUpgradeEvent(upgradeContext.property);
                     break;
                case UIType.DiceRoll:
                      HandleDiceRollPannel();
                      break;
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
                //keep consistent w rent system
                if (controlledPlayer.ExecutePurchase(pac.Property, pac.Property.buyPrice) == Player.FinancialStatus.Success)
                {
                    pac.Property.SetOwner(controlledPlayer);
                }

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

            RequestResolutionComplete();
        }

        // This forces the end turn request to be validated the same way as any other player action,
        // This makes sure that end turn can be blocked by effects that prevent the player from ending
        // their turn. This is also a workaround to separate AwaitingResolution and Completed states.
        private void OnTurnEnded(bool ended)
        {
            if (!isMyTurn) return;

            RequestTurnAction(
                TurnActionType.EndTurn,
                onAllowed: () =>
                {
                    Logger.Debug("HumanPlayerController.RequestEndTurn",
                        "End turn approved by TurnFlowCoordinator.",
                        LogCategory.Gameplay);
                },
                onDenied: () =>
                {
                    Logger.Debug("HumanPlayerController.RequestEndTurn",
                        "End turn denied by TurnFlowCoordinator.",
                        LogCategory.Gameplay);
                });
        }

        private void HandleDiceRollPannel()
        {
            if (!isMyTurn) return;

            Logger.Debug("HumanPlayerController.HandleDiceRollePannel",
                       "Dice Roll Pannel Reached.",
                       LogCategory.UI);
            RequestTurnAction(
                TurnActionType.RollDice,
                onAllowed: () =>
                {
                    uiActivationEventChannel?.RaiseEvent(
                        new UIActivationEvent(
                            UIType.DiceRoll,
                            new PurchaseActivationContext(
                                /*TODO: update input fields below*/
                                null,
                                0,
                                controlledPlayer.CanAfford(0))));
                    Logger.Debug("HumanPlayerController.HandleDiceRollPannel",
                       "Dice Roll Pannel Allowed.",
                       LogCategory.UI);
                },
                onDenied: () =>
                {
                    Logger.Debug("HumanPlayerController.HandleDiceRollPannel",
                        " Dice Roll Pannel UI blocked by TurnFlow.",
                        LogCategory.UI);
                });
        }

       
    }
}