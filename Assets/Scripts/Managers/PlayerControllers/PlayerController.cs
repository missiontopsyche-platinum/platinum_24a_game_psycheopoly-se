using Assets.Scripts.Managers.TurnFlow;
using Events.EventDataStructures;
using Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.PlayerControllers
{
    public class PlayerController
    {
        // fields
        protected readonly Player controlledPlayer;
        protected bool isMyTurn = false;

        //gets the player scriptable object
        public Player GetControlledPlayer() => controlledPlayer;

        private readonly TurnStartedEventChannel turnStartedEventChannel;
        protected readonly PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
        protected readonly ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;
        protected readonly PayPlayerEventChannel passedGoPaymentChannel;
        protected readonly TurnActionRequestEventChannel turnActionRequestEventChannel;
        protected readonly TurnActionResultEventChannel turnActionResultEventChannel;

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
            PurchaseOwnableRequestEventChannel purchaseRequest, 
            ChargeOwnershipFeeEventChannel chargeOwnershipFee, 
            PayPlayerEventChannel passedGoPayment,
            TurnActionRequestEventChannel turnActionRequest,
            TurnActionResultEventChannel  turnActionResult)
        {
            controlledPlayer = player ?? throw new System.ArgumentNullException(nameof(player));
            turnStartedEventChannel = turnStarted ?? throw new System.ArgumentNullException(nameof(turnStarted));
            purchaseOwnableRequestEventChannel =
                purchaseRequest ?? throw new System.ArgumentNullException(nameof(purchaseRequest));
            chargeOwnershipFeeEventChannel =
                chargeOwnershipFee ?? throw new System.ArgumentNullException(nameof(chargeOwnershipFee));
            passedGoPaymentChannel = passedGoPayment ?? throw new System.ArgumentNullException(nameof(passedGoPayment));
            turnActionRequestEventChannel = turnActionRequest ?? 
                throw new System.ArgumentNullException(nameof(turnActionRequest));
            turnActionResultEventChannel = turnActionResult ?? 
                throw new System.ArgumentNullException(nameof(turnActionResult));
        }
        
        /// <summary>
        /// Subscribe to basic game event channels. This should be extended in subclasses to capture
        /// the distinct methods and behavior events, by calling <c>base.Subscribe()</c>.
        /// </summary>
        public virtual void Subscribe()
        {
            turnStartedEventChannel?.Subscribe(CatchTurnStartedEvent);
            turnActionResultEventChannel?.Subscribe(OnTurnActionResult);
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
                Logging.Logger.Debug("PlayerController.RequestTurnAction",
                    $"Ignored duplicate pending request: {action}",
                    LogCategory.UI);
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

        
    }
}