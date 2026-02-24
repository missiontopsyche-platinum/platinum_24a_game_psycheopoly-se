using AIBehavior;
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
        // private AIMortgageBehavior
        // private AIJailBehavior
        // etc...
        
        // event channels ... I don't think this will need special ones

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
            PurchaseOwnableRequestEventChannel purchaseRequest,
            ChargeOwnershipFeeEventChannel chargeOwnershipFee,
            PayPlayerEventChannel passedGoPayment,
            TurnActionRequestEventChannel turnActionRequest,
            TurnActionResultEventChannel turnActionResult) 
            : base(player, turnStarted, purchaseRequest, chargeOwnershipFee, passedGoPayment, turnActionRequest, turnActionResult)
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
            // mortgageBehavior
            // jailBehavior etc...
        }

        public override void Subscribe()
        {
            base.Subscribe();
            purchaseOwnableRequestEventChannel?.Subscribe(PurchaseRequestDecision);
            chargeOwnershipFeeEventChannel?.Subscribe(HandleChargeOwnershipFee);
            passedGoPaymentChannel?.Subscribe(HandlePassedGo);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            purchaseOwnableRequestEventChannel?.Unsubscribe(PurchaseRequestDecision);
            chargeOwnershipFeeEventChannel?.Unsubscribe(HandleChargeOwnershipFee);
            passedGoPaymentChannel?.Unsubscribe(HandlePassedGo);
        }
        
        protected override void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            base.CatchTurnStartedEvent(tse);
            
            // start turn decision flows
            // check for upgrades
            // roll dice
            // resolve movement (purchase, pay rent, jail, etc)
            // check for upgrades
            
            // the turn flow might need to operate as a Coroutine that waits on completion flags.
        }

        private void HandleUpgradeAction()
        {
            AIUpgradeEvaluation evaluation;
            do
            {
                evaluation = upgradeBehavior.EvaluateUpgrade();

                if (evaluation.willUpgrade)
                {
                    // handle purchase upgrade for property on evaluation.upgradeTarget
                    // need to figure out the upgrade flow for this more concretely...
                    
                    Logger.Info("AIPlayerController.HandleUpgradeAction",
                        $"Executing upgrade on {evaluation.upgradeTarget.spaceName}",
                        LogCategory.AI);
                }
            } while (evaluation.willUpgrade);
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
            
            // handle paying rent
        }

        private void HandlePassedGo(PayPlayerEvent ppe)
        {
            if (!isMyTurn) return;
            
            // handle passing go
        }
    }
}