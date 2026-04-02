using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Assets.Scripts.Managers.TurnFlow;
using Logging;
using System;
using System.Collections.Generic;

namespace Managers.PlayerControllers
{
    public class PlayerController
    {
        // fields
        protected readonly Player controlledPlayer;
        protected bool isMyTurn = false;

        //gets the player scriptable object
        public Player GetControlledPlayer() => controlledPlayer;
        
        // event channels || may need to add more as requirements change. Potentially have all channels in all subclasses.
        private TurnStartedEventChannel turnStartedEventChannel;
        protected BooleanEventChannel turnEndedEventChannel;
        protected PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
        protected ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;
        protected PayPlayerEventChannel passedGoPaymentChannel;
        protected CardDrawnEventChannel cardDrawnEventChannel;
        protected TurnActionRequestEventChannel turnActionRequestEventChannel;
        protected TurnActionResultEventChannel turnActionResultEventChannel;
        protected UpgradeRequestEventChannel upgradeRequestEventChannel;
        protected IntEventChannel bankruptPlayerEventChannel;
        protected JailStateChangedEventChannel jailStateChangedEventChannel;
        protected IntEventChannel forcedTurnAdvanceEventChannel;


        // These handle the callbacks for when a turn action request is allowed or denied from the TurnFlowCoordinator.
        private struct PendingCallbacks
        {
            public Action Allowed;
            public Action Denied;
        }
        private readonly Dictionary<TurnActionType, PendingCallbacks> pendingActions = new();

        // constructor, needs to be called in future subclass constructors with `super(args)`
        public PlayerController(
            Player player, 
            TurnStartedEventChannel turnStarted, 
            BooleanEventChannel turnEnded,
            PurchaseOwnableRequestEventChannel purchaseRequest, 
            ChargeOwnershipFeeEventChannel chargeOwnershipFee, 
            PayPlayerEventChannel passedGoPayment,
            UpgradeRequestEventChannel upgradeRequest,
            TurnActionRequestEventChannel turnActionRequest,
            TurnActionResultEventChannel  turnActionResult,
            IntEventChannel bankruptPlayer,
            JailStateChangedEventChannel jailStateChanged)
        {
            controlledPlayer = player ?? throw new System.ArgumentNullException(nameof(player));
            turnStartedEventChannel = turnStarted ?? throw new System.ArgumentNullException(nameof(turnStarted));
            turnEndedEventChannel = turnEnded ?? throw new System.ArgumentNullException(nameof(turnEnded));
            purchaseOwnableRequestEventChannel =
                purchaseRequest ?? throw new System.ArgumentNullException(nameof(purchaseRequest));
            chargeOwnershipFeeEventChannel =
                chargeOwnershipFee ?? throw new System.ArgumentNullException(nameof(chargeOwnershipFee));
            passedGoPaymentChannel = passedGoPayment ?? throw new System.ArgumentNullException(nameof(passedGoPayment));
            turnActionRequestEventChannel = turnActionRequest ?? 
                throw new System.ArgumentNullException(nameof(turnActionRequest));
            turnActionResultEventChannel = turnActionResult ?? 
                throw new System.ArgumentNullException(nameof(turnActionResult));
            upgradeRequestEventChannel = upgradeRequest ?? throw new System.ArgumentNullException(nameof(upgradeRequest));
            bankruptPlayerEventChannel = bankruptPlayer ?? throw new System.ArgumentException(nameof(bankruptPlayer));
            jailStateChangedEventChannel = jailStateChanged ?? throw new System.ArgumentNullException(nameof(jailStateChanged));
        }
        
        /// <summary>
        /// Subscribe to basic game event channels. This should be extended in subclasses to capture
        /// the distinct methods and behavior events, by calling <c>base.Subscribe()</c>.
        /// </summary>
        public virtual void Subscribe()
        {
            turnStartedEventChannel?.Subscribe(CatchTurnStartedEvent);
            turnActionResultEventChannel?.Subscribe(OnTurnActionResult);
            jailStateChangedEventChannel?.Subscribe(HandleJailStateChanged);
        }

        /// <summary>
        /// Unsubscribe from all game event channels. This should be extended in subclasses to account for
        /// subclass-specific event channels and called with <c>base.Unsubscribe()</c>.
        /// clean up if not returned
        /// </summary>
        public virtual void Unsubscribe()
        {
            turnStartedEventChannel?.Unsubscribe(CatchTurnStartedEvent);
            turnActionResultEventChannel?.Unsubscribe(OnTurnActionResult);
            jailStateChangedEventChannel?.Unsubscribe(HandleJailStateChanged);
            pendingActions.Clear();
            isMyTurn = false;
        }
        
        /// <summary>
        /// Catches the TurnStartedEvent, and evaluates if it is this Player's turn or not, and sets the
        /// boolean state accordingly (<c>isMyTurn</c>).
        /// </summary>
        /// <param name="tse">Turn Started Event, containing payload information about the new turn.</param>
        protected virtual void CatchTurnStartedEvent(TurnStartedEvent tse)
        {
            isMyTurn = tse.playerId == controlledPlayer.GetId();
            
            if (isMyTurn)
                Logging.Logger.Info("PlayerController.CatchTurnStartedEvent",
                    $"Player {controlledPlayer.GetId()} turn started.",
                    LogCategory.Gameplay, this);
        }

        /// <summary>
        /// Sends a turn action request to TurnFlow to check if the
        /// controlled player is allowed to perform the action.
        ///
        /// Only one pending request per action type is allowed at a time.
        /// Duplicate requests are ignored.
        ///
        /// If approved, <paramref name="onAllowed"/> is invoked.
        /// If denied, <paramref name="onDenied"/> is invoked (optional).
        /// </summary>
        /// <param name="action">The action being requested.</param>
        /// <param name="onAllowed">Callback executed if the action is allowed.</param>
        /// <param name="onDenied">Optional callback executed if the action is denied.</param>
        protected void RequestTurnAction(
            TurnActionType action,
            Action onAllowed,
            Action onDenied = null)
        {
            if (onAllowed == null) return;

            if (pendingActions.ContainsKey(action))
            {
                Logger.Debug(
                    "PlayerController.RequestTurnAction",
                    $"Ignored duplicate pending request for action {action}.",
                    LogCategory.Gameplay,
                    this);
                return;
            }


            pendingActions[action] = new PendingCallbacks
            {
                Allowed = onAllowed,
                Denied = onDenied
            };

            turnActionRequestEventChannel?.RaiseEvent(new TurnActionRequest
            {
                player = controlledPlayer,
                action = action
            });
        }

        /// <summary>
        /// Handles the result of a turn action request from TurnFlow.
        ///
        /// If the result matches the controlled player and a pending
        /// request exists, the appropriate callback (allowed or denied)
        /// is executed. The pending request is then removed.
        /// </summary>
        /// <param name="result">The result of the requested turn action.</param>
        protected void OnTurnActionResult(TurnActionResult result)
        {
            if (result.playerId != controlledPlayer.GetId())
                return;

            if (!pendingActions.TryGetValue(result.action, out var pending))
                return;

            pendingActions.Remove(result.action);

            if (result.allowed) pending.Allowed?.Invoke();
            else pending.Denied?.Invoke();
        }

        /// <summary>
        /// Tells the TurnFlow system that the resolution part of the 
        /// turn is done. This is a helper method for completing resolution 
        /// using TurnActionType.CompleteResolution.
        /// </summary>
        /// <param name="onAllowed">Runs if the action is allowed.</param>
        /// <param name="onDenied">Runs if the action is denied.</param>
        /// 
        protected void RequestResolutionComplete(Action onAllowed = null, Action onDenied = null)
        {
            RequestTurnAction(
                TurnActionType.CompleteResolution,
                onAllowed ?? (() => { }),
                onDenied);
        }

        /// <summary>
        /// Applies jail state changes to the controlled player.
        /// If this player is actively taking their turn and is sent to jail,
        /// raise a forced turn advance event instead of directly
        /// calling TurnFlowCoordinator.
        /// </summary>
        protected virtual void HandleJailStateChanged(JailStateChangedEvent jailEvent)
        {
            if (jailEvent == null || jailEvent.player == null)
                return;

            if (jailEvent.player.GetId() != controlledPlayer.GetId())
                return;

            controlledPlayer.SetInJail(jailEvent.inJail);
            controlledPlayer.SetJailTurns(jailEvent.jailTurns);

            Logging.Logger.Info("PlayerController.HandleJailStateChanged",
                $"Updated jail state for {controlledPlayer.GetPName()}: inJail={jailEvent.inJail}, jailTurns={jailEvent.jailTurns}.",
                LogCategory.Gameplay,
                this);

            if (isMyTurn && jailEvent.inJail)
            {
                pendingActions.Clear();
                isMyTurn = false;
            }
        }
    }
}