using System;
using System.Collections.Generic;
using System.Linq;
using AIBehavior;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Managers.TurnFlow;
using Data;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using Logging;
using UnityEngine;
using Logger = Logging.Logger;

namespace Managers.PlayerControllers
{
    public class AIPlayerController: PlayerController
    {
        // Behavior Weights (personality)
        private readonly AIBehaviorWeights weights;
        
        // Behavior Classes
        private readonly AIPurchaseBehavior purchaseBehavior;
        private readonly AIUpgradeBehavior upgradeBehavior;
        private readonly AIMortgageBehavior mortgageBehavior;
        private readonly AIUnmortgageBehavior unmortgageBehavior;
        
        // state variables
        private bool endTurnRequested;
        private bool hasRolled;
        private bool isInDebt;

        // optional action variables
        private Queue<Action> optionalActionsQueue;
        private int upgradesExecuted;

        /// <summary>
        /// Creates an AI player controller. This needs to be called in conjunction with <c>.Subscribe()</c>
        /// so that event channels are properly routed.
        /// </summary>
        public AIPlayerController(
            Player player,
            AIBehaviorWeights aiBehaviorWeights,
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
            moneyDistribution)
        {
            // load in behavior / personality
            weights = aiBehaviorWeights;
            
            // create the behavior classes
            purchaseBehavior = new AIPurchaseBehavior(
                controlledPlayer, 
                weights.purchaseWeights, 
                weights.purchaseThresholds);
            upgradeBehavior = new AIUpgradeBehavior(
                controlledPlayer,
                weights.upgradeWeights,
                weights.upgradeThresholds);
            mortgageBehavior = new AIMortgageBehavior(
                controlledPlayer,
                weights.mortgageThresholds);
            unmortgageBehavior = new AIUnmortgageBehavior(
                controlledPlayer,
                weights.mortgageThresholds);
        }

        public override void Subscribe()
        {
            base.Subscribe();
            uiActionEventChannel?.Subscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel?.Subscribe(PurchaseRequestDecision);
            chargeOwnershipFeeEventChannel?.Subscribe(HandleChargeOwnershipFee);
            passedGoPaymentChannel?.Subscribe(HandlePassedGo);
            moneyDistributionEventChannel?.Subscribe(HandleMoneyDistribution);
            chargePlayerEventChannel?.Subscribe(HandleChargePlayerEvent);
            noLandingActionEventChannel?.Subscribe(HandleNoLandingActionEvent);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            uiActionEventChannel?.Unsubscribe(HandleUIAction);
            purchaseOwnableRequestEventChannel?.Unsubscribe(PurchaseRequestDecision);
            chargeOwnershipFeeEventChannel?.Unsubscribe(HandleChargeOwnershipFee);
            passedGoPaymentChannel?.Unsubscribe(HandlePassedGo);
            moneyDistributionEventChannel?.Unsubscribe(HandleMoneyDistribution);
            chargePlayerEventChannel?.Unsubscribe(HandleChargePlayerEvent);
            noLandingActionEventChannel?.Unsubscribe(HandleNoLandingActionEvent);
        }
        
        protected override void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            base.CatchTurnStartedEvent(tse);

            if (!isMyTurn) return;

            endTurnRequested = false;
            hasRolled = false;
            isInDebt = false;
            
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.TurnStartedBanner, new TurnStartedBannerContext(
                    controlledPlayer,
                    () =>
                    {
                        if (controlledPlayer.IsInJail())
                        {
                            HandleJailTurnStart();
                            return;
                        }

                        ExecuteOptionalActions();
                    }, isAI:true)));
        }

        private void ExecuteOptionalActions()
        {
            optionalActionsQueue = new Queue<Action>();
            optionalActionsQueue.Enqueue(ExecuteUpgradePhase);
            optionalActionsQueue.Enqueue(ExecuteUnmortgagePhase);
            optionalActionsQueue.Enqueue(ExecuteMortgagePhase);

            NextOptionalAction();
        }

        private void NextOptionalAction()
        {
            if(optionalActionsQueue.Count > 0)
                optionalActionsQueue.Dequeue().Invoke();
            else
                OnOptionalActionsComplete();
        }

        private void ExecuteUpgradePhase()
        {
            upgradesExecuted = 0;
            RequestTurnAction(
                TurnActionType.ModifyProperty,
                HandleUpgradeAction, 
                NextOptionalAction);
        }

        private void ExecuteUnmortgagePhase()
        {
            RequestTurnAction(
                TurnActionType.ModifyProperty,
                HandleUnmortgageAction, 
                NextOptionalAction);
        }

        private void ExecuteMortgagePhase()
        {
            RequestTurnAction(
                TurnActionType.ModifyProperty,
                HandleMortgageAction, 
                NextOptionalAction);
        }

        private void OnOptionalActionsComplete()
        {
            if (hasRolled)
                RequestEndTurn();
            else
                ExecuteDiceRoll();
        }

        private void HandleUIAction(UIActionEvent uiae)
        {
            if (!isMyTurn) return;

            switch (uiae.UIType)
            {
                case UIType.GeneralNotification:
                    if (uiae.Context is GeneralAcknowledgement context)
                        context.onAcknowledged.Invoke();
                    break;
            }
        }

        private void ExecuteDiceRoll()
        {
            RequestTurnAction(
                TurnActionType.RollDice,
                () =>
                {
                    uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                        UIType.DiceRoll,
                        new DiceRollPanelContext(isAI:true)));
                    Logger.Info("AIPlayerController.StartNormalRollFlow",
                        $"AI {controlledPlayer.GetPName()} rolled dice.",
                        LogCategory.AI);
                    hasRolled = true;
                }, () => Logger.Error(
                    "AIPlayerController.ExecuteDiceRoll",
                    $"{controlledPlayer.GetPName()}: Dice Roll denied!", 
                    LogCategory.AI));
        }

        private void HandleUpgradeAction()
        {
            AIUpgradeEvaluation evaluation = upgradeBehavior.EvaluateUpgrade();

            if (!evaluation.willUpgrade)
            {
                if (upgradesExecuted == 0)
                {
                    NextOptionalAction();
                    return;
                }
                
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, new GeneralNotificationContext(
                        controlledPlayer,
                        "Upgrade Actions Taken",
                        $"{controlledPlayer.GetPName()} upgraded {upgradesExecuted} properties.",
                        NextOptionalAction,
                        isAI: true)));
                return;
            }
            evaluation.upgradeTarget.UpgradeProperty();
            controlledPlayer.TrySpend(evaluation.upgradeTarget.UpgradeCost);
            upgradesExecuted++;
            HandleUpgradeAction(); // recurse until we chose to not upgrade
        }

        private void HandleMortgageAction()
        {
            AIMortgageEvaluation evaluation = mortgageBehavior.EvaluateMortgage();
            int dataPointsSold = 0;
            int discoveriesSold = 0;
            int propsMortgaged = 0;

            foreach (var mortgageAction in evaluation.actions)
            {
                switch (mortgageAction.actionType)
                {
                    case MortgageActionType.Mortgage:
                        if (controlledPlayer.MortgageProperty(mortgageAction.ownableSpace))
                        {
                            Logger.Info("AIPlayerController.HandleMortgageAction",
                                $"AI {controlledPlayer.GetPName()} mortgaged {mortgageAction.ownableSpace.spaceName}.",
                                LogCategory.AI);
                            propsMortgaged++;
                        }
                        else
                        {
                            Logger.Error("AIPlayerController.HandleMortgageAction",
                                $"AI {controlledPlayer.GetPName()} failed to mortgage {mortgageAction.ownableSpace.spaceName}.",
                                LogCategory.AI);
                        }
                        break;
                    case MortgageActionType.SellDataPoint:
                    {
                        if (mortgageAction.ownableSpace is PropertySpaceData prop)
                        {
                            controlledPlayer.AddMoney(prop.SellDataPoint());
                            dataPointsSold++;
                        }
                        break;
                    }
                    case MortgageActionType.SellDiscovery:
                    {
                        if (mortgageAction.ownableSpace is PropertySpaceData prop)
                        {
                            controlledPlayer.AddMoney(prop.SellDiscovery());
                            discoveriesSold++;
                        }
                        break;
                    }
                }
            }
            Logger.Info("AIPlayerController.HandleMortgageAction",
                $"Mortgage Evaluation: {evaluation.message}.",
                LogCategory.AI);

            Action continuation = isInDebt
                ? () =>
                {
                    isInDebt = false;
                    RequestAIResolutionComplete();
                }
                : NextOptionalAction;
            
            // there were no actions completed, continue
            if (dataPointsSold == 0 && propsMortgaged == 0 && discoveriesSold == 0)
            { 
                continuation.Invoke();
                return;
            }

            // else we tell the player what happened.
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, new GeneralNotificationContext(
                    controlledPlayer,
                    "Mortgage Actions Taken",
                    $"{controlledPlayer.GetPName()} has mortgaged {propsMortgaged} properties, " +
                    $"has sold {dataPointsSold} data points, and " +
                    $"sold {discoveriesSold} discoveries.",
                    continuation,
                    true)));
        }

        private void HandleUnmortgageAction()
        {
            AIUnmortgageEvaluation evaluation = unmortgageBehavior.EvaluateUnmortgage();
            int unmortgagedProps = 0;

            foreach (var action in evaluation.actions)
            {
                if (action.actionType != MortgageActionType.Unmortgage)
                    continue;

                bool success = controlledPlayer.UnmortgageProperty(action.ownableSpace);

                if (success)
                {
                    Logger.Info("AIPlayerController.HandleUnmortgageAction",
                        $"AI {controlledPlayer.GetPName()} unmortgaged {action.ownableSpace.spaceName}.",
                        LogCategory.AI);
                    unmortgagedProps++;
                }
                else
                {
                    Logger.Warn("AIPlayerController.HandleUnmortgageAction",
                        $"AI {controlledPlayer.GetPName()} failed to unmortgage {action.ownableSpace.spaceName}.",
                        LogCategory.AI);
                }
            }
            Logger.Info("AIPlayerController.HandleUnmortgageAction",
                $"Unmortgage Evaluation: {evaluation.message}.",
                LogCategory.AI);

            if (unmortgagedProps == 0)
            {
                NextOptionalAction();
                return;
            }
            
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, new GeneralNotificationContext(
                    controlledPlayer,
                    "Unmortgaging Actions Taken",
                    $"{controlledPlayer.GetPName()} has unmortgaged " +
                    $"{unmortgagedProps} properties.",
                    NextOptionalAction,
                    isAI:true)));
        }

        private void PurchaseRequestDecision(PurchaseOwnableRequestEvent pore)
        {
            if (!isMyTurn) return;

            bool shouldPurchase = purchaseBehavior.EvaluatePurchase(pore.requestedSpace);

            if (shouldPurchase)
            {
                controlledPlayer.ExecutePurchase(
                    pore.requestedSpace, 
                    pore.requestedSpace.buyPrice);
                
                Logger.Info("AIPlayerController.PurchaseRequestDecision",
                    $"Computer Player {controlledPlayer.GetPName()} has executed purchase on {pore.requestedSpace.spaceName}",
                    LogCategory.AI);
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, new GeneralNotificationContext(
                        controlledPlayer,
                        "Property Purchased!",
                        $"{controlledPlayer.GetPName()} bought {pore.requestedSpace.spaceName} " +
                        $"for ${pore.cost}.",
                        RequestAIResolutionComplete,
                        true)));
            }
            else
            {
                Logger.Info("AIPlayerController.PurchaseRequestDecision",
                    $"Computer Player {controlledPlayer.GetPName()} has declined purchase on {pore.requestedSpace.name}",
                    LogCategory.AI);
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, new GeneralNotificationContext(
                        controlledPlayer,
                        "Purchase Declined!",
                        $"{controlledPlayer.GetPName()} has declined purchase on {pore.requestedSpace.spaceName}.",
                        RequestAIResolutionComplete,
                        true)));
            }
        }

        private void HandleSpendMoney(int amount)
        {
            Player.FinancialStatus status = controlledPlayer.TrySpend(amount);
            switch (status)
            {
                case Player.FinancialStatus.Success:
                    Logger.Info("AIPlayerController.HandleSpendMoney",
                        $"AI {controlledPlayer.GetPName()} paid fee of ${amount}.",
                        LogCategory.AI);
                    break;

                case Player.FinancialStatus.Bankrupt:
                    Logger.Warn("AIPlayerController.HandleSpendMoney",
                        $"AI {controlledPlayer.GetPName()} cannot pay fee and is bankrupt.",
                        LogCategory.AI);

                    bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                    break;

                case Player.FinancialStatus.MortgageRequired:
                    Logger.Info("AIPlayerController.HandleSpendMoney",
                        $"AI {controlledPlayer.GetPName()} needs mortgage handling to cover fee of ${amount}.",
                        LogCategory.AI);
                    isInDebt = true;
                    HandleMortgageAction();
                    break;
            }
        } 

        private void HandleChargePlayerEvent(ChargePlayerEvent cpe)
        {
            if (!isMyTurn || cpe.chargedPlayer != controlledPlayer) return;
            
            HandleSpendMoney(cpe.chargeAmount);
            
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, new GeneralNotificationContext(
                    controlledPlayer,
                    "Charged Fee!",
                    $"{controlledPlayer.GetPName()} has been charged ${cpe.chargeAmount}",
                    RequestAIResolutionComplete,
                    true)));
        }

        private void HandleChargeOwnershipFee(ChargeOwnershipFeeEvent cofe)
        {
            if (!isMyTurn) return;

            HandleSpendMoney(cofe.amount);
            
            cofe.toPlayer.AddMoney(cofe.amount);
            
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, new GeneralNotificationContext(
                    controlledPlayer,
                    "Charged Fee!",
                    $"{controlledPlayer.GetPName()} has been charged ${cofe.amount}, " +
                    $"paid to {cofe.toPlayer.GetPName()}.",
                    RequestAIResolutionComplete,
                    true)));
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            if (!isMyTurn) return;

            if (ppe == null || ppe.paidPlayer == null)
            {
                Logger.Error("AIPlayerController.HandlePassedGo",
                    "Received null PayPlayerEvent or player payload.",
                    LogCategory.AI);
                return;
            }

            if (ppe.paidPlayer.GetId() != controlledPlayer.GetId())
                return;

            // handle passing go
            controlledPlayer.AddMoney(ppe.amountPaid);

            RequestAIResolutionComplete();
        }

        // Handles CollectFromAllPlayers card effects for the AI player's controller.
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
                            RequestResolutionComplete();
                            break;

                        case Player.FinancialStatus.MortgageRequired:
                            // reuse existing AI mortgage handling.
                            HandleMortgageAction();
                            break;
                    }
                }
            }
            // if actualAmount is 0, then we are being paid and don't need to do anything
            // regardless, if it is our turn we need to mark resolution complete
            if (isMyTurn)
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, new GeneralNotificationContext(
                        controlledPlayer,
                        "Money distribution!",
                        $"{controlledPlayer.GetPName()} has executed on the card.",
                        RequestAIResolutionComplete,
                        true)));
        }
        
        private void HandleNoLandingActionEvent(NoActionLandingEvent noActionLanding)
        {
            if (!isMyTurn) return;
            uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                UIType.GeneralNotification, 
                new GeneralNotificationContext(controlledPlayer,
                    noActionLanding.spaceName,
                    noActionLanding.flavorText,
                    RequestAIResolutionComplete,
                    isAI: true)));
        }

        private void RequestAIResolutionComplete()
        {
            RequestTurnAction(
                TurnActionType.CompleteResolution,
                OnLandingActionResolved,
                () =>
                {
                    Logger.Error("AIPlayerController.RequestAIResolutionComplete",
                        $"{controlledPlayer.GetPName()} was denied resolution complete.",
                        LogCategory.AI);
                });
        }

        private void OnLandingActionResolved()
        {
            if (controlledPlayer.IsInJail())
            {
                RequestEndTurn();
                return;
            }
            
            ExecuteOptionalActions();
        }

        private void RequestEndTurn()
        {
            if (endTurnRequested) return;

            endTurnRequested = true;

            RequestTurnAction(
                TurnActionType.EndTurn,
                onAllowed: () =>
                {
                    Logger.Info("AIPlayerController.RequestEndTurn",
                        $"{controlledPlayer.GetPName()} has ended their turn.",
                        LogCategory.AI);
                },
                onDenied: () =>
                {
                    Logger.Error("AIPlayerController.RequestEndTurn",
                        $"{controlledPlayer.GetPName()} was denied ending of turn.",
                        LogCategory.AI);
                });
        }

        private enum AIJailDecision
        {
            RollForEscape,
            PayFine,
            UseCard
        }

        private void HandleJailTurnStart()
        {
            AIJailDecision decision = EvaluateJailDecision();

            Logger.Info("AIPlayerController.HandleJailTurnStart",
                $"AI {controlledPlayer.GetPName()} jail decision: {decision}",
                LogCategory.AI);

            switch (decision)
            {
                case AIJailDecision.UseCard:
                    HandleUseJailCard();
                    break;

                case AIJailDecision.PayFine:
                    HandlePayJailFine();
                    break;

                case AIJailDecision.RollForEscape:
                default:
                    HandleRollForJailEscape();
                    break;
            }
        }

        private AIJailDecision EvaluateJailDecision()
        {
            bool hasJailCard =
                controlledPlayer.GetChanceCardCount() > 0 ||
                controlledPlayer.GetCommunityCardCount() > 0;

            bool canAffordFine = controlledPlayer.CanAfford(Assets.Scripts.Managers.Jail.JailUtility.JAIL_FEE);
            int nextJailTurn = controlledPlayer.GetJailTurns() + 1;

            //On 3rd attempt prefer  the guaranteed exit methods
            if (nextJailTurn >= Assets.Scripts.Managers.Jail.JailUtility.MAX_TURNS_IN_JAIL)
            {
                if (hasJailCard)
                    return AIJailDecision.UseCard;

                if (canAffordFine)
                    return AIJailDecision.PayFine;

                return AIJailDecision.RollForEscape;
            }

            //use a card first, then pay only if cash allows and if not roll.
            if (hasJailCard)
                return AIJailDecision.UseCard;

            if (canAffordFine && controlledPlayer.GetMoney() > (Assets.Scripts.Managers.Jail.JailUtility.JAIL_FEE + 200))
                return AIJailDecision.PayFine;

            return AIJailDecision.RollForEscape;
        }

        //The methods below help wire jail decisions to JailUtility
        private void HandleRollForJailEscape()
        {
            RequestTurnAction(
                TurnActionType.RollForJailEscape,
                onAllowed: () =>
                {
                    Logger.Info("AIPlayerController.HandleRollForJailEscape",
                        $"AI {controlledPlayer.GetPName()} is rolling for jail escape.",
                        LogCategory.AI);
                },
                onDenied: () =>
                {
                    Logger.Warn("AIPlayerController.HandleRollForJailEscape",
                        $"AI {controlledPlayer.GetPName()} was denied RollForJailEscape.",
                        LogCategory.AI);
                });
        }

        private void HandlePayJailFine()
        {
            var result = Assets.Scripts.Managers.Jail.JailUtility.PayFee(controlledPlayer);

            Logger.Info("AIPlayerController.HandlePayJailFine",
                $"AI {controlledPlayer.GetPName()} pay fine: {result}",
                LogCategory.AI);

            if (result == Assets.Scripts.Managers.Jail.JailUtility.FeePaymentResult.Paid)
            {
                RaiseJailStateChanged(false);
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, new GeneralNotificationContext(
                        controlledPlayer,
                        "Paid Launch Pad Fee!",
                        $"{controlledPlayer.GetPName()} has paid the fee to leave the launch pad.",
                        ExecuteOptionalActions,
                        true)));
                return;
            }

            bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
            RequestEndTurn();
        }

        private void HandleUseJailCard()
        {
            var result = Assets.Scripts.Managers.Jail.JailUtility.UseGetOutOfJailFree(controlledPlayer);

            Logger.Info("AIPlayerController.HandleUseJailCard",
                $"AI {controlledPlayer.GetPName()} use card result: {result}",
                LogCategory.AI);

            if (result == Assets.Scripts.Managers.Jail.JailUtility.CardUseResult.Success)
            {
                RaiseJailStateChanged(false);
                uiActivationEventChannel.RaiseEvent(new UIActivationEvent(
                    UIType.GeneralNotification, new GeneralNotificationContext(
                        controlledPlayer,
                        "Left the Launch Pad using Card!",
                        $"{controlledPlayer.GetPName()} has left the launch pad with a card.",
                        ExecuteOptionalActions,
                        true)));
                return;
            }

            HandleRollForJailEscape();
        }

        private void RaiseJailStateChanged(bool inJail)
        {
            jailStateChangedEventChannel?.RaiseEvent(
                new Assets.Scripts.Events.EventDataStructures.JailStateChangedEvent(
                    controlledPlayer,
                    inJail,
                    controlledPlayer.GetJailTurns()));
        }
    }
}