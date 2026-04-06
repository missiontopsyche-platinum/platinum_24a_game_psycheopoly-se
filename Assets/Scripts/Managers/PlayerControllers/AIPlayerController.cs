using AIBehavior;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Managers.TurnFlow;
using Data;
using Events.EventDataStructures;
using Logging;

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
        // private AIJailBehavior
        // etc...
        
        private ActionResolvedEventChannel actionResolvedEventChannel;
        private BooleanEventChannel diceRollRequestChannel;

        /// <summary>
        /// Creates an AI player controller. This needs to be called in conjunction with <c>.Subscribe()</c>
        /// so that event channels are properly routed.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="aiBehaviorWeights"></param>
        /// <param name="turnStarted"></param>
        /// <param name="purchaseRequest"></param>
        /// <param name="chargeOwnershipFee"></param>
        /// <param name="passedGoPayment"></param>
        public AIPlayerController(
            Player player,
            AIBehaviorWeights aiBehaviorWeights,
            TurnStartedEventChannel turnStarted,
            BooleanEventChannel turnEnded,
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment,
            BooleanEventChannel diceRollRequest,
            ActionResolvedEventChannel actionResolved,
            UpgradeRequestEventChannel upgradeRequest,
            IntEventChannel bankruptPlayer,
            TurnActionRequestEventChannel turnActionRequest,
            TurnActionResultEventChannel turnActionResult,
            JailStateChangedEventChannel jailStateChanged) 
            : base(player, turnStarted, turnEnded, purchaseRequest, chargeOwnershipFee, passedGoPayment, upgradeRequest, turnActionRequest, turnActionResult, bankruptPlayer, jailStateChanged)
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
            actionResolvedEventChannel = actionResolved ?? throw new System.ArgumentNullException(nameof(actionResolved));
            diceRollRequestChannel = diceRollRequest ?? throw new System.ArgumentNullException(nameof(diceRollRequest));
            // mortgageBehavior
            // jailBehavior etc...
        }

        public override void Subscribe()
        {
            base.Subscribe();
            purchaseOwnableRequestEventChannel?.Subscribe(PurchaseRequestDecision);
            chargeOwnershipFeeEventChannel?.Subscribe(HandleChargeOwnershipFee);
            passedGoPaymentChannel?.Subscribe(HandlePassedGo);
            actionResolvedEventChannel?.Subscribe(OnActionResolved);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            purchaseOwnableRequestEventChannel?.Unsubscribe(PurchaseRequestDecision);
            chargeOwnershipFeeEventChannel?.Unsubscribe(HandleChargeOwnershipFee);
            passedGoPaymentChannel?.Unsubscribe(HandlePassedGo);
            actionResolvedEventChannel?.Unsubscribe(OnActionResolved);
        }
        
        protected override void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            base.CatchTurnStartedEvent(tse);

            if (!isMyTurn) return;
            
            HandleOptionalActions();

            // AI must roll dice or the game stalls at AwaitingRoll.
            RequestTurnAction(
                TurnActionType.RollDice,
                onAllowed: () =>
                {
                    if (diceRollRequestChannel == null)
                    {
                        Logger.Error("AIPlayerController.CatchTurnStartedEvent",
                            "DiceRollRequestChannel not found. AI cannot roll dice.",
                            LogCategory.AI);
                        return;
                    }

                    // TODO: ensure this ACTUALLY WORKS.... because right now it 100% does not.
                    diceRollRequestChannel.RaiseEvent(true);

                    Logger.Info("AIPlayerController.CatchTurnStartedEvent",
                        $"AI {controlledPlayer.GetPName()} rolled dice.",
                        LogCategory.AI);
                },
                onDenied: () =>
                {
                    Logger.Warn("AIPlayerController.CatchTurnStartedEvent",
                        $"AI {controlledPlayer.GetPName()} attempted RollDice but was denied.",
                        LogCategory.AI);
                });
            
            // dice roll resolution is handled by catching other events to continue turn flow.
        }

        private void HandleOptionalActions()
        {
            HandleMortgageAction();
            // handle unmortgage goes here
            HandleUpgradeAction();
        }

        // turning this into a recursive method so that it works with our TFC validation scheme
        private void HandleUpgradeAction()
        {
            AIUpgradeEvaluation evaluation = upgradeBehavior.EvaluateUpgrade();

            // early exit if no valid upgrade
            if (!evaluation.willUpgrade || evaluation.upgradeTarget == null) return;

            PropertySpaceData target = evaluation.upgradeTarget;
            var monopolyGroup = controlledPlayer.GetOwnedPropertiesByColor(target.groupColor);
            var decision = UpgradeUtility.Evaluate(controlledPlayer, target, monopolyGroup.ToArray());

            RequestTurnAction(
                TurnActionType.ModifyProperty,
                onAllowed: () =>
                {
                    if (UpgradeUtility.TryExecute(controlledPlayer, target, decision))
                    {
                        Logger.Info("AIPlayerController.HandleUpgradeAction",
                            $"Executed upgrade on {target.spaceName}.",
                            LogCategory.AI);
                        
                        HandleUpgradeAction(); // recurse until no more upgrades to complete
                    }
                    else
                    {
                        Logger.Warn("AIPlayerController.HandleUpgradeAction",
                            $"Upgrade failed to execute on {target.spaceName}, validation error.",
                            LogCategory.AI);
                    }
                },
                onDenied: () =>
                {
                    Logger.Debug("AIPlayerController.HandleUpgradeAction",
                        $"Upgrade request denied for {target.spaceName}.",
                        LogCategory.AI);
                }
            );
        }

        private void HandleMortgageAction()
        {
            AIMortgageEvaluation evaluation = mortgageBehavior.EvaluateMortgage();

            if (evaluation.actions.Count > 0)
            {
                RequestTurnAction(
                    TurnActionType.ModifyProperty,
                    onAllowed: () =>
                    {
                        ExecuteMortgageActions(evaluation);
                    },
                    onDenied: () =>
                    {
                        Logger.Warn("AIPlayerController.HandleMortgageAction",
                            $"AI Mortgage actions not allowed at this phase in the game!",
                            LogCategory.AI);
                    }
                );
            }
        }

        private void HandleForcedMortgage()
        {
            AIMortgageEvaluation evaluation = mortgageBehavior.EvaluateMortgage();
            
            // this happens outside normal turn phase because we are reconciling debt,
            // and we know there are mortgage actions to take, because otherwise we'd
            // be bankrupt and the turn would end immediately.
            ExecuteMortgageActions(evaluation);
        }

        private void ExecuteMortgageActions(AIMortgageEvaluation evaluation)
        {
            foreach (var mortgageAction in evaluation.actions)
            {
                switch (mortgageAction.actionType)
                {
                    case MortgageActionType.Mortgage:
                        if (controlledPlayer.MortgageProperty(mortgageAction.ownableSpace))
                        {
                            Logger.Info("AIPlayerController.ExecuteMortgageActions",
                                $"AI {controlledPlayer.GetPName()} mortgaged {mortgageAction.ownableSpace.spaceName}.",
                                LogCategory.AI);
                        }
                        else
                        {
                            Logger.Warn("AIPlayerController.ExecuteMortgageActions",
                                $"AI {controlledPlayer.GetPName()} failed to mortgage {mortgageAction.ownableSpace.spaceName}.",
                                LogCategory.AI);
                        }
                        break;
                    case MortgageActionType.SellDataPoint:
                        // TODO: sell data point
                        Logger.Warn("AIPlayerController.ExecuteMortgageActions",
                            $"Sell Data Point Not Implemented!",
                            LogCategory.AI);
                        break;
                    case MortgageActionType.SellDiscovery:
                        // TODO: sell discovery (all upgrades for this property)
                        Logger.Warn("AIPlayerController.ExecuteMortgageActions",
                            $"Sell Discovery Not Implemented!",
                            LogCategory.AI);
                        break;
                }
            }
            Logger.Info("AIPlayerController.ExecuteMortgageActions",
                $"Mortgage Evaluation: {evaluation.message}.",
                LogCategory.AI);
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
                    $"Computer Player {controlledPlayer.GetPName()} has executed purchase on {pore.requestedSpace.name}",
                    LogCategory.AI);
            }
            else
            {
                // we don't need to do much for declining a purchase, besides maybe flagging that 
                // the Movement phase is resolved for the AI turn flow.
                
                Logger.Info("AIPlayerController.PurchaseRequestDecision",
                    $"Computer Player {controlledPlayer.GetPName()} has declined purchase on {pore.requestedSpace.name}",
                    LogCategory.AI);
            }
        }

        private void HandleChargeOwnershipFee(ChargeOwnershipFeeEvent cofe)
        {
            if (!isMyTurn) return;

            Player.FinancialStatus status = controlledPlayer.TrySpend(cofe.amount);

            switch (status)
            {
                case Player.FinancialStatus.Success:
                    Logger.Info("AIPlayerController.HandleChargeOwnershipFee",
                        $"AI {controlledPlayer.GetPName()} paid ownership fee of ${cofe.amount}.",
                        LogCategory.AI);
                    break;

                case Player.FinancialStatus.Bankrupt:
                    Logger.Warn("AIPlayerController.HandleChargeOwnershipFee",
                        $"AI {controlledPlayer.GetPName()} cannot pay ownership fee and is bankrupt.",
                        LogCategory.AI);

                    bankruptPlayerEventChannel?.RaiseEvent(controlledPlayer.GetId());
                    break;

                case Player.FinancialStatus.MortgageRequired:
                    Logger.Info("AIPlayerController.HandleChargeOwnershipFee",
                        $"AI {controlledPlayer.GetPName()} needs mortgage handling to cover ownership fee of ${cofe.amount}.",
                        LogCategory.AI);
                    HandleForcedMortgage();
                    break;
            }

            RequestResolutionComplete();
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            if (!isMyTurn) return;

            // handle passing go
            RequestResolutionComplete();
        }

        private void OnActionResolved(ActionResolvedEvent evt)
        {
            if (evt.playerId != controlledPlayer.GetId()) return;

            // TODO: Any post-action logic or decision-making would go here before ending the turn.


            // After the AI player moves, it will skip AwaitingResolution phase and request to end turn.
            // This is with the assumption that the AI will be doing all of its decision-making and action
            // execution above, so there won't be any need to wait for resolution before ending the turn.
            RequestResolutionComplete(
                onAllowed: RequestEndTurn,
                onDenied: RequestEndTurn);
        }

        private void RequestEndTurn()
        {
            RequestTurnAction(
                TurnActionType.EndTurn,
                onAllowed: () => { /* nothing needs to happen here, since isMyTurn bool will be flipped on new turn start. */ },
                onDenied: () =>
                {
                    Logger.Warn("AIPlayerController.RequestEndTurn",
                        $"AI {controlledPlayer.GetPName()} EndTurn request denied. " +
                        $"End of turn reached out of sync with Turn Flow Coordinator.",
                        LogCategory.AI);
                });
        }
    }
}