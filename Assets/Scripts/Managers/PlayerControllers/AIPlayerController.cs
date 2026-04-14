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
        private readonly AIUnmortgageBehavior unmortgageBehavior;

        // private AIJailBehavior
        // etc...
        private bool myTurnActive;
        private bool endTurnRequested;
        // event channels ... I don't think this will need special ones
        private ActionResolvedEventChannel actionResolvedEventChannel;
        private BooleanEventChannel diceRollRequestChannel;
        private MortgageFinishedEventChannel mortgageFinishedEventChannel;

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
            JailStateChangedEventChannel jailStateChanged,
            MortgageFinishedEventChannel mortgageFinishedEventChannel,
            ChargePlayerEventChannel chargePlayer,
            NoActionLandingEventChannel noLandingAction)
            : base(
                player,
                turnStarted,
                turnEnded,
                purchaseRequest,
                chargeOwnershipFee,
                passedGoPayment,
                upgradeRequest,
                turnActionRequest,
                turnActionResult,
                bankruptPlayer,
                jailStateChanged,
                chargePlayer,
                noLandingAction)
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

            actionResolvedEventChannel = actionResolved ?? throw new System.ArgumentNullException(nameof(actionResolved));
            diceRollRequestChannel = diceRollRequest ?? throw new System.ArgumentNullException(nameof(diceRollRequest));
            // mortgageBehavior
            this.mortgageFinishedEventChannel =
                mortgageFinishedEventChannel ?? throw new System.ArgumentNullException(nameof(mortgageFinishedEventChannel));
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
            // TODO: This needs to be implemented and property integrated. Currently the actions don't *do* anything.
            base.CatchTurnStartedEvent(tse);

            if (!isMyTurn) return;

            myTurnActive = true;
            endTurnRequested = false;
            
            HandleOptionalActions();

            // TODO This might need to run as a coroutine so that we can await the completed movement
            // alternatively, we'd need to fully decouple this from the loop and have separate methods... but
            // to be honest, having an AI Turn coroutine makes a lot of sense- keep it all in the same logical
            // place.

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
            // wait for resolution of the movement phase (land on space, resolve space)

            HandleOptionalActions();
            // end turn
        }

        private void HandleOptionalActions()
        {
            HandleMortgageAction();
            // handle unmortgage goes here
            HandleUnmortgageAction();
            HandleUpgradeAction();
        }

        private void HandleUpgradeAction()
        {
            AIUpgradeEvaluation evaluation;

            do
            {
                evaluation = upgradeBehavior.EvaluateUpgrade();

                if (!evaluation.willUpgrade || evaluation.upgradeTarget == null)
                    break;

                PropertySpaceData target = evaluation.upgradeTarget;

                RequestTurnAction(
                    TurnActionType.ModifyProperty,
                    onAllowed: () =>
                    {
                        upgradeRequestEventChannel?.RaiseEvent(
                            new UpgradeRequestEvent(controlledPlayer, target));

                        Logger.Info("AIPlayerController.HandleUpgradeAction",
                            $"Raised upgrade request for {target.spaceName}.",
                            LogCategory.AI);
                    },
                    onDenied: () =>
                    {
                        Logger.Debug("AIPlayerController.HandleUpgradeAction",
                            $"Upgrade request denied for {target.spaceName}.",
                            LogCategory.AI);
                    });

                break;

            } while (evaluation.willUpgrade);
        }

        private void HandleMortgageAction()
        {
            AIMortgageEvaluation evaluation = mortgageBehavior.EvaluateMortgage();

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
                        }
                        else
                        {
                            Logger.Warn("AIPlayerController.HandleMortgageAction",
                                $"AI {controlledPlayer.GetPName()} failed to mortgage {mortgageAction.ownableSpace.spaceName}.",
                                LogCategory.AI);
                        }
                        break;
                    case MortgageActionType.SellDataPoint:
                        // sell data point
                        break;
                    case MortgageActionType.SellDiscovery:
                        // sell discovery (all upgrades for this property)
                        break;
                }
            }
            Logger.Info("AIPlayerController.HandleMortgageAction",
                $"Mortgage Evaluation: {evaluation.message}.",
                LogCategory.AI);
        }

        private void HandleUnmortgageAction()
        {
            AIUnmortgageEvaluation evaluation = unmortgageBehavior.EvaluateUnmortgage();

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

                    mortgageFinishedEventChannel?.RaiseEvent(
                        new MortgageFinishedEvent(controlledPlayer, action.ownableSpace));
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
                    HandleMortgageAction();
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
            if (!myTurnActive || endTurnRequested) return;

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
            if (endTurnRequested) return;

            endTurnRequested = true;

            RequestTurnAction(
                TurnActionType.EndTurn,
                onAllowed: () =>
                {
                    myTurnActive = false;
                },
                onDenied: () =>
                {
                    endTurnRequested = false;
                });
        }
    }
}