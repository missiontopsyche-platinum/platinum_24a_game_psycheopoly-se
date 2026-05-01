using System.Linq;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;

using Assets.Scripts.Managers.Jail;
using Assets.Scripts.Managers.TurnFlow;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using Logging;

using Logger = Logging.Logger;


namespace Managers.PlayerControllers
{
    public class HumanPlayerController : PlayerController
    {
        /// <summary>
        /// Creates Human Player controller. Once called, it must have <c>Subscribe()</c> called on it to ensure
        /// all event channels are properly subscribed.
        /// </summary>
        public HumanPlayerController(
            Player player,
            TurnStartedEventChannel turnStarted,
            BooleanEventChannel turnEnded,
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment,
            TurnActionRequestEventChannel turnActionRequest,
            TurnActionResultEventChannel turnActionResult,
            UpgradeRequestEventChannel upgradeRequest,
            IntEventChannel bankruptPlayer,
            JailStateChangedEventChannel jailStateChanged,
            ChargePlayerEventChannel chargePlayer,
            NoActionLandingEventChannel noLandingAction,
            UIActivationEventChannel uiActivation,
            UIActionEventChannel uiAction,
            MoneyDistributionEventChannel moneyDistribution
            ) : base(player, 
                turnStarted, 
                turnEnded, 
                purchaseRequest,
                chargeOwnershipFee,
                passedGoPayment, 
                turnActionRequest,
                turnActionResult,
                upgradeRequest, 
                bankruptPlayer, 
                jailStateChanged, 
                chargePlayer, 
                noLandingAction,
                uiActivation,
                uiAction,
                moneyDistribution) { }

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
            chargePlayerEventChannel?.Subscribe(HandleChargePlayer);
            noLandingActionEventChannel?.Subscribe(HandleNoLandingActionEvent);
            jailStateChangedEventChannel?.Subscribe(HandleJailStateChanged);
            moneyDistributionEventChannel?.Subscribe(HandleMoneyDistribution);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            uiActionEventChannel?.Unsubscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel?.Unsubscribe(HandlePurchaseOwnableEvent);
            chargeOwnershipFeeEventChannel?.Unsubscribe(HandleChargeOwnership);
            passedGoPaymentChannel?.Unsubscribe(HandlePassedGo);
            turnEndedEventChannel?.Unsubscribe(OnTurnEnded);
            chargePlayerEventChannel?.Unsubscribe(HandleChargePlayer);
            noLandingActionEventChannel?.Unsubscribe(HandleNoLandingActionEvent);
            jailStateChangedEventChannel?.Unsubscribe(HandleJailStateChanged);
            moneyDistributionEventChannel?.Unsubscribe(HandleMoneyDistribution);
        }
        
        //PlayerController already established turn ownership, HumanPlayerController handles human-player
        //specific decisions/flow
        protected override void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            base.CatchTurnStartedEvent(tse);

            if (!isMyTurn)
                return;
            
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.TurnStartedBanner, new TurnStartedBannerContext(
                    controlledPlayer,
                    () =>
                    {
                        if (!controlledPlayer.IsInJail())
                            return;

                        ShowJailOptionsUI();
                    })));
        }

        private void HandlePurchaseOwnableEvent(PurchaseOwnableRequestEvent pore)
        {
            if (!isMyTurn || turnForcedEnd) return;
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
            if (!isMyTurn || property == null || turnForcedEnd) 
                return;

            RequestTurnAction(
                TurnActionType.ModifyProperty,
                onAllowed: () =>
                {
                    UpgradeManager.TryHandleUpgrade(controlledPlayer, property, out UpgradeDecision decision);

                    ReopenPropertyManagementUI();

                    Logger.Debug("HumanPlayerController.HandleUpgradeEvent",
                        $"{controlledPlayer.GetPName()} attempted to upgrade {property.spaceName}: " +
                        $"{(decision.Allowed ? "success" : "failure")}.",
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
            if (!isMyTurn || turnForcedEnd) return;
            
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

            HandleNoLandingActionEvent(new NoActionLandingEvent(cofe.sourceSpace.GetShortName(), $"Paid {cofe.amount} to {cofe.toPlayer.name} in rent!"));
                
                
            RequestResolutionComplete();
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            // TODO refactor this with a more specific event type that can distinguish between passing and landing on GO.
            // passing go shouldn't fire a notification UI, but landing on it should, but we currently have no good way
            // of knowing if we've passed it or landed on it.
            
            if (!isMyTurn || turnForcedEnd) return;

            // ADDED: guard against malformed event payloads.
            if (ppe == null || ppe.paidPlayer == null)
            {
                Logger.Error("HumanPlayerController.HandlePassedGo",
                    "Received null PayPlayerEvent or player payload.",
                    LogCategory.Gameplay);
                return;
            }

            if (ppe.paidPlayer.GetId() != controlledPlayer.GetId())
                return;

            // handle passing go
            // call player method for getting paid for passing go
            controlledPlayer.AddMoney(ppe.amountPaid);

            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, new GeneralNotificationContext(
                    controlledPlayer,
                    "GO!!!",
                    $"{controlledPlayer.GetPName()} collected ${ppe.amountPaid}.",
                    () => RequestResolutionComplete())));
        }

        // handles CollectFromAllPlayers card effects for the human player's controller.
        private void HandleMoneyDistribution(MoneyDistributionEvent mde)
        {
            // skip players already removed / marked bankrupt.
            if (controlledPlayer.IsMarkedBankrupt())
                return;
            
            if (mde == null || mde.Player == null)
                return;

            // ignore invalid card values.
            if (mde.Amount <= 0)
                return;

            int actualAmount = mde.Amount;

            bool isCardHolder = mde.Player.GetId() == controlledPlayer.GetId();

            if (isCardHolder)
            {
                int activePlayers = PlayerManager.GetInstance()
                    .GetAllPlayers()
                    .Count(p => !p.IsMarkedBankrupt());
                actualAmount = mde.Type switch
                {
                    // this player is paying- mult input amount by active players
                    MoneyDistributionEvent.MoneyDistributionEventType.Pay => mde.Amount * activePlayers, 
                    // this player is collecting
                    MoneyDistributionEvent.MoneyDistributionEventType.Collect => 0, 
                    _ => 0
                };
            }

            if (actualAmount > 0)
            {
                // if this is a Pay All Players event and is not the cardholder, add money
                if (mde.Type == MoneyDistributionEvent.MoneyDistributionEventType.Pay 
                    && !isCardHolder)
                    controlledPlayer.AddMoney(actualAmount);
                else // otherwise, we can handle the charging based on actual amount
                {
                    // charge player this amount and resolve debt if necessary
                    var status = controlledPlayer.TrySpend(actualAmount);
                
                    switch (status)
                    {
                        case Player.FinancialStatus.Success:
                            // successful payment goes to the player who drew the card.
                            mde.Player.AddMoney(mde.Amount);
                            break;

                        case Player.FinancialStatus.Bankrupt:
                            // notify existing bankruptcy flow.
                            bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                            break;

                        case Player.FinancialStatus.MortgageRequired:
                            // reuse existing AI mortgage handling.
                            controlledPlayer.SetMoney(controlledPlayer.GetMoney() - mde.Amount);
                            HandleDebtResolution();
                            break;
                    }
                }
            }
            // if actualAmount is 0, then we are being paid and don't need to do anything
            // regardless, if it is our turn we need to mark resolution complete
            if (isMyTurn)
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, 
                    new GeneralNotificationContext(controlledPlayer,
                        "Card resolution complete!",
                        $"Money distribution has been completed.",
                        () => RequestResolutionComplete())));
        }

        private void HandleDebtResolution()
        {
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.PropertyManagement,
                new PropertyManagementActivationContext(
                    controlledPlayer,
                    true,
                    controlledPlayer.GetMoney())));
        }

        private void ResolveMortgageProperty(MortgagePropertyContext context)
        {
            if (!isMyTurn || turnForcedEnd || context == null)
                return;

            if (controlledPlayer.MortgageProperty(context.tile))
            {
                // this whole event channel and event payload are used *nowhere else* in the system
                // mortgageFinishedEventChannel?.RaiseEvent(new MortgageFinishedEvent(
                //     controlledPlayer,
                //     context.tile));
                ReopenPropertyManagementUI();
            }

            RequestResolutionComplete();
        }

        private void HandleUIAction(UIActionEvent uiae)
        {
            if (!isMyTurn) return;
            
            switch (uiae.UIType)
            {
                case UIType.PropertyPurchase:
                            if (uiae.Context is PurchaseActionContext purchaseContext)
                                ResolvePropertyPurchase(purchaseContext);
                            else
                                Logger.Error("HumanPlayerController.HandleUIAction",
                                    $"Expected PurchaseActionContext but got {uiae.Context?.GetType().Name}",
                                    LogCategory.UI);
                            break;

                        case UIType.MortgagePropertySelected:
                            if (uiae.Context is MortgagePropertyContext mortgageContext)
                                ResolveMortgageProperty(mortgageContext);
                            else
                                Logger.Error("HumanPlayerController.HandleUIAction",
                                    $"Expected MortgagePropertyContext but got {uiae.Context?.GetType().Name}",
                                    LogCategory.UI);
                            break;

                        case UIType.UnmortgagePropertySelected:
                            if (uiae.Context is UnmortgagePropertyContext unmortgageContext)
                                ResolveUnmortgageProperty(unmortgageContext);
                            else
                                Logger.Error("HumanPlayerController.HandleUIAction",
                                    $"Expected UnmortgagePropertyContext but got {uiae.Context?.GetType().Name}",
                                    LogCategory.UI);
                            break;

                        case UIType.PropertyUpgradeSelected:
                            if (uiae.Context is PropertyUpgradeContext upgradeContext)
                                HandleUpgradeEvent(upgradeContext.property);
                            break;

                        case UIType.PropertyDowngradeSelected:
                            if (uiae.Context is PropertyDowngradeContext downgradeContext)
                                HandleDowngradeEvent(downgradeContext.Property);
                            break;

                        case UIType.JailOptions:
                            if (uiae.Context is JailActionContext jailContext)
                                ResolveJailAction(jailContext);
                            else
                                Logger.Error("HumanPlayerController.HandleUIAction",
                                    $"Expected JailActionContext but got {uiae.Context?.GetType().Name}",
                                    LogCategory.UI);
                            break;

                        case UIType.DiceRoll:
                            HandleDiceRollPannel();
                            break;

                        case UIType.GeneralNotification:
                            HandleGeneralNotificationAcknowledgement((GeneralAcknowledgement)uiae.Context);
                            break;

                        default:
                            Logger.Debug("HumanPlayerController.HandleUIAction",
                                $"Unhandled UI Type: {uiae.UIType}",
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
                    $"{controlledPlayer.GetPName()} has executed purchase on ${pac.Property.spaceName}",
                    LogCategory.Gameplay); 
                //Fire
            }
            else
            {
                // controlledPlayer.DeclinePurchase(pac.Property);
                Logger.Debug("HumanPlayerController.ResolvePropertyPurchase",
                    $"{controlledPlayer.GetPName()} has declined purchase on ${pac.Property.spaceName}",
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
            Logger.Debug("HumanPlayerController.RequestEndTurn",
                        "Checking that the player turn isn't cycling somehow.",
                        LogCategory.Gameplay);
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
            if (!isMyTurn || turnForcedEnd) return;

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
                            new DiceRollPanelContext(isAI: false)));
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

        private void HandleChargePlayer(ChargePlayerEvent cpe)
        {
            if (!isMyTurn || turnForcedEnd) return;

            Player.FinancialStatus status = controlledPlayer.TrySpend(cpe.chargeAmount);

            if (status == Player.FinancialStatus.Success)
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, 
                    new GeneralNotificationContext(controlledPlayer,
                        "Charged Fee!",
                        $"You have been charged ${cpe.chargeAmount}.",
                        () => RequestResolutionComplete())));
            else if (status == Player.FinancialStatus.Bankrupt)
            {
                bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, 
                    new GeneralNotificationContext(controlledPlayer,
                        "Bankrupt!",
                        $"You have gone bankrupt.",
                        () => RequestResolutionComplete())));
            }
            else
            {
                controlledPlayer.SetMoney(controlledPlayer.GetMoney() - cpe.chargeAmount);
                HandleDebtResolution();
            }
        }

        private void HandleNoLandingActionEvent(NoActionLandingEvent noActionLanding)
        {
            if (!isMyTurn) return;
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, 
                new GeneralNotificationContext(controlledPlayer,
                    noActionLanding.spaceName,
                    noActionLanding.flavorText,
                    () => RequestResolutionComplete())));
        }

        private void HandleGeneralNotificationAcknowledgement(GeneralAcknowledgement ga)
        {
            ga.onAcknowledged.Invoke();
        }

        // show you're in jail notification
        protected override void HandleJailStateChanged(JailStateChangedEvent jailEvent)
        {
            if (!isMyTurn) return;
            
            base.HandleJailStateChanged(jailEvent);

            if (jailEvent.inJail)
            {
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification,
                    new GeneralNotificationContext(controlledPlayer,
                        "LAUNCH PAD!",
                        "You're now stuck at the launch pad!",
                        () => 
                            RequestTurnAction(
                                TurnActionType.CompleteResolution,
                                onAllowed: () => RequestTurnAction(
                                    TurnActionType.EndTurn,
                                    onAllowed: () => { },
                                    onDenied: () => { }
                        ), 
                                onDenied: () => { }
                        ))));
            }
            else
            {
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification,
                    new GeneralNotificationContext(controlledPlayer,
                        "GO FOR LAUNCH!",
                        "You're no longer stuck on the Launch Pad!",
                        () => RequestResolutionComplete())));
            }
        }

        //display jail options for the current human player
        private void ShowJailOptionsUI()
        {
            bool hasGetOutOfJailCard = controlledPlayer.GetJailCards().Count > 0;
            
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
                            uiActivationEventChannel?.RaiseEvent(new UIActivationEvent(
                                UIType.DiceRoll, new DiceRollPanelContext(isAI: controlledPlayer.isAI)));
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

        private void ReopenPropertyManagementUI()
        {
            uiActivationEventChannel?.RaiseEvent(
                new UIActivationEvent(
                    UIType.PropertyManagement,
                    new PropertyManagementActivationContext(controlledPlayer)
                )
            );
        }

        private void ResolveUnmortgageProperty(UnmortgagePropertyContext context)
        {
            if (!isMyTurn || turnForcedEnd || context == null)
                return;

            if (controlledPlayer.UnmortgageProperty(context.Tile))
            {
                ReopenPropertyManagementUI();
            }

            RequestResolutionComplete();
        }

        private void HandleDowngradeEvent(PropertySpaceData property)
        {
            if (!isMyTurn || turnForcedEnd || property == null)
                return;

            RequestTurnAction(
                TurnActionType.ModifyProperty,
                onAllowed: () =>
                {
                    if (property.CanDowngrade())
                    {
                        // handle discovery (hotel) vs data point (house)
                        controlledPlayer.AddMoney(property.UpgradeLevel == 5
                            ? property.SellDiscovery()
                            : property.SellDataPoint());
                        ReopenPropertyManagementUI();
                    }

                    RequestResolutionComplete();
                },
                onDenied: () =>
                {
                    Logger.Debug("HumanPlayerController.HandleDowngradeEvent",
                        "Downgrade blocked by TurnFlow.",
                        LogCategory.UI);
                });
        }
    }
}