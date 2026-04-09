using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Managers.TurnFlow;
using Assets.Scripts.Managers.Jail;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using Logging;
using Logger = Logging.Logger;
using UnityEngine;


namespace Managers.PlayerControllers
{
    public class HumanPlayerController : PlayerController
    {
        // attributes

        // event channels
        private readonly UIActivationEventChannel uiActivationEventChannel;
        private readonly UIActionEventChannel uiActionEventChannel;
        private readonly MortgageFinishedEventChannel mortgageFinishedEventChannel;


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
            JailStateChangedEventChannel jailStateChanged) 
            : base(player, turnStarted, turnEnded, purchaseRequest, chargeOwnershipFee, passedGoPayment, upgradeRequest, turnActionRequest, turnActionResult, bankruptPlayer, jailStateChanged)
        {
            // human controller specific setup goes here
            uiActivationEventChannel = uiActivation;
            uiActionEventChannel = uiAction;
            mortgageFinishedEventChannel = mortgageFinished;
            bankruptPlayerEventChannel = bankruptPlayer;
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

        //PlayerController already established turn ownership, HumanPlayerController handles human-player
        //specific decisions/flow
        protected override void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            base.CatchTurnStartedEvent(tse);

            if (!isMyTurn)
                return;

            if (!controlledPlayer.IsInJail())
                return;

            ShowJailOptionsUI();
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
                    
                    RefreshPropertyManagementUI();

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

            Player.FinancialStatus status = controlledPlayer.TrySpend(cofe.amount);

            switch (status)
            {
                case Player.FinancialStatus.Success:
                    break;

                case Player.FinancialStatus.Bankrupt:
                    bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                    break;

                case Player.FinancialStatus.MortgageRequired:
                    uiActivationEventChannel?.RaiseEvent(
                        new UIActivationEvent(
                            UIType.PropertyManagement,
                            new PropertyManagementActivationContext(
                                controlledPlayer,
                                true,
                                Mathf.Max(0, cofe.amount - controlledPlayer.GetMoney())
                            )));
                    break;
            }

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
            if (!isMyTurn || context == null || context.tile == null) return;

            if (controlledPlayer.MortgageProperty(context.tile))
            {
                mortgageFinishedEventChannel?.RaiseEvent(new MortgageFinishedEvent(
                    controlledPlayer,
                    context.tile));
            }

            int debtRemaining = Mathf.Max(0, -controlledPlayer.GetMoney());
            RefreshPropertyManagementUI(debtRemaining > 0, debtRemaining);
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
                case UIType.PropertyManagement:
                    if (uiae.Context is PropertyDowngradeContext downgradeContext)
                        ResolvePropertyDowngrade(downgradeContext);
                    else if (uiae.Context is UnmortgagePropertyContext unmortgageContext)
                        ResolveUnmortgageProperty(unmortgageContext);
                    else
                        Logger.Error("HumanPlayerController.HandleUIAction",
                            $"Expected PropertyManagement action context but got {uiae.Context?.GetType().Name}",
                            LogCategory.UI);
                    break;
                case UIType.JailOptions:
                    if (uiae.Context is JailActionContext jailContext)
                        ResolveJailAction(jailContext);
                    else
                        Logger.Error("HumanPlayerController.HandleUIAction",
                            $"Expected JailActionContext but got {uiae.Context?.GetType().Name}",
                            LogCategory.UI);
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

        //handles player-selected jail actions by routing to JailUtility OR requesting a TFC-controlled escape roll 
        // done to keep dice ownership out of PlayerController
        private void ResolveJailAction(JailActionContext context)
        {
            if (!isMyTurn || context == null)
                return;

            switch (context.Choice)
            {
                case JailChoice.PayFine:
                    ResolveJailFinePayment();
                    break;

                case JailChoice.UseCard:
                    ResolveJailCardUse();
                    break;

                case JailChoice.RollForEscape:
                    RequestTurnAction(
                        TurnActionType.RollForJailEscape,
                        onAllowed: () =>
                        {
                            Logger.Info("HumanPlayerController.ResolveJailAction",
                                $"{controlledPlayer.GetPName()} requested a jail escape roll.",
                                LogCategory.Gameplay);
                        },
                        onDenied: () =>
                        {
                            Logger.Info("HumanPlayerController.ResolveJailAction",
                                $"{controlledPlayer.GetPName()} was denied a jail escape roll.",
                                LogCategory.Gameplay);
                        });
                    break;
            }
        }

        //attempts to pay jail fine and raise bakruptcy is player can't pay
        private void ResolveJailFinePayment()
        {
            JailUtility.FeePaymentResult result = JailUtility.PayFee(controlledPlayer);

            Logger.Info("HumanPlayerController.ResolveJailFinePayment",
                $"{controlledPlayer.GetPName()} jail fine result: {result}.",
                LogCategory.Gameplay);

            if (result == JailUtility.FeePaymentResult.Bankrupt)
            {
                bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
            }
        }

        //uses the GOOJFCard if the player has it, logs the result.
        private void ResolveJailCardUse()
        {
            JailUtility.CardUseResult result = JailUtility.UseGetOutOfJailFree(controlledPlayer);

            Logger.Info("HumanPlayerController.ResolveJailCardUse",
                $"{controlledPlayer.GetPName()} jail card result: {result}.",
                LogCategory.Gameplay);

            
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

       private void RefreshPropertyManagementUI(bool debtMode = false, int debtAmount = 0)
        {
            uiActivationEventChannel?.RaiseEvent(
                new UIActivationEvent(
                    UIType.PropertyManagement,
                    new PropertyManagementActivationContext(controlledPlayer, debtMode, debtAmount)));
        }

        private void ResolveUnmortgageProperty(UnmortgagePropertyContext context)
        {
            if (!isMyTurn || context == null || context.Tile == null) return;

            controlledPlayer.UnmortgageProperty(context.Tile);
            RefreshPropertyManagementUI();
            RequestResolutionComplete();
        }

        private void ResolvePropertyDowngrade(PropertyDowngradeContext context)
        {
            if (!isMyTurn || context == null || context.Property == null) return;

            PropertySpaceData property = context.Property;
            if (!controlledPlayer.GetValidDowngradableProperties().Contains(property))
            {
                RequestResolutionComplete();
                return;
            }
            int currentLevel = property.GetCurrentUpgradeLevel();

            if (currentLevel <= 0)
            {
                RequestResolutionComplete();
                return;
            }

            int refund = property.GetUpgradeCostForLevel(currentLevel) / 2;
            property.SetUpgradeLevel(currentLevel - 1);
            controlledPlayer.AddMoney(refund);

            int debtRemaining = Mathf.Max(0, -controlledPlayer.GetMoney());
            RefreshPropertyManagementUI(debtRemaining > 0, debtRemaining);
            RequestResolutionComplete();
        }

        //display jail options for the current human player
        private void ShowJailOptionsUI()
        {
            bool hasGetOutOfJailCard =
                controlledPlayer.GetChanceCardCount() > 0 ||
                controlledPlayer.GetCommunityCardCount() > 0;


            uiActivationEventChannel?.RaiseEvent(
                new UIActivationEvent(
                    UIType.JailOptions,
                    new JailActivationContext(
                        controlledPlayer.GetPName(),
                        controlledPlayer.GetJailTurns(),
                        JailUtility.MAX_TURNS_IN_JAIL,
                        controlledPlayer.CanAfford(JailUtility.JAIL_FEE),
                        hasGetOutOfJailCard,
                        JailUtility.JAIL_FEE)));

            Logger.Info("HumanPlayerController.ShowJailOptionsUI",
                $"{controlledPlayer.GetPName()} is in jail. Showing jail options UI.",
                LogCategory.UI);
        }
    }
}