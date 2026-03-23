using Assets.Scripts.Managers.TurnOrder;
using Logging;
using UnityEngine;

namespace Assets.Scripts.Managers.TurnFlow
{
    /// <summary>
    /// centralized version for a turn progression controller.
    /// Listens to roll > movement > tile effects > resolution
    /// determines next player using TurnCycleManager + PlayerTurnState.
    /// </summary>
    public class TurnFlowCoordinator : MonoBehaviour
    {
        [Header("Event Channels In")]
        [SerializeField] private TurnStartedEventChannel turnStartedInChannel;
        [SerializeField] private DiceRolledEventChannel diceRolledChannel;
        [SerializeField] private BooleanEventChannel pieceMoveCompletedChannel;
        [SerializeField] private TurnActionRequestEventChannel turnActionRequestChannel;
        [SerializeField] private TurnActionResultEventChannel turnActionResultChannel;

        [Header("Event Channels Out")]
        [SerializeField] private ActionResolvedEventChannel actionResolvedEventChannel;
        [SerializeField] private TurnStartedEventChannel turnStartedOutChannel;

        [Header("Dependencies")]
        [SerializeField] private TurnCycleManager turnCycleManager;

        public TurnPhase Phase { get; private set; } = TurnPhase.None;
        public int ActivePlayer { get; private set; } = -1;
        // prevents double-advance in same turn
        private bool awaitingEndTurn = false;

        public void Initialize(TurnCycleManager tcm)
        {
            if (turnCycleManager == null)
            {
                turnCycleManager = tcm;
            }
        }

        /// <summary>
        /// This will be called by the GameManager after setup is complete. This will
        /// kick off the turn loop by emitting the first TurnStartedEvent. After this,
        /// the TurnFlowCoordinator will listen to events and manage turn progression on its own.
        /// </summary>
        public void StartGame()
        {
            if (turnCycleManager == null)
            {
                Logging.Logger.Error("TurnFlowCoordinator.StartFirstTurn",
                    "TurnCycleManager is missing. Cannot start turn loop.",
                    LogCategory.Core,
                    this);
                return;
            }

            int startingPlayer = turnCycleManager.CurrentPlayerIndex;
            ActivePlayer = startingPlayer;
            Phase = TurnPhase.AwaitingRoll;
            awaitingEndTurn = false;
            turnStartedOutChannel?.RaiseEvent(new TurnStartedEvent(startingPlayer, 0));
        }

        private void OnEnable()
        {
            turnStartedInChannel?.Subscribe(OnTurnStarted);
            diceRolledChannel?.Subscribe(OnDiceRolled);
            pieceMoveCompletedChannel?.Subscribe(OnPieceMoveCompleted);
            turnActionRequestChannel?.Subscribe(OnTurnActionRequested);
        }

        private void OnDisable()
        {
            turnStartedInChannel?.Unsubscribe(OnTurnStarted);
            diceRolledChannel?.Unsubscribe(OnDiceRolled);
            pieceMoveCompletedChannel?.Unsubscribe(OnPieceMoveCompleted);
            turnActionRequestChannel?.Unsubscribe(OnTurnActionRequested);
        }

        // a new turn's started, reset state, wait for new roll
        private void OnTurnStarted(TurnStartedEvent data)
        {
            ActivePlayer = data.playerId;
            Phase = TurnPhase.AwaitingRoll;
            awaitingEndTurn = false;

            // This is a sanity check to make sure our TurnCycleManager is in sync with the turn
            if (turnCycleManager != null && turnCycleManager.CurrentPlayerIndex != data.playerId)
            {
                Logging.Logger.Warn("TurnFlowCoordinator.OnTurnStarted",
                    $"Mismatch: TurnStarted player={data.playerId} but TurnCycle={turnCycleManager.CurrentPlayerIndex}. Syncing.",
                    Logging.LogCategory.Core,
                    this);

                turnCycleManager.SyncCurrentPlayerIndex(data.playerId);
            }
        }

        // after dice roll, wait for movement to contine
        private void OnDiceRolled(DiceRolledEvent diceEvent)
        {
            if (Phase != TurnPhase.AwaitingRoll) return;
            Phase = TurnPhase.AwaitingMovement;
        }


        // essenitally "complete resolution" part
        private void OnPieceMoveCompleted(bool success)
        {
            if (!success) return;
            if (Phase != TurnPhase.AwaitingMovement) return;

            Phase = TurnPhase.AwaitingResolution;

            actionResolvedEventChannel?.RaiseEvent(new ActionResolvedEvent(ActivePlayer));
        }


        // decide if another turn is in order or if we can move onto next plater
        private void CompleteTurnFlow()
        {
            if (!awaitingEndTurn) return;
            if (turnCycleManager == null)
            {
                Logging.Logger.Error("TurnFlowCoordinator.CompleteTurnFlow",
                    "TurnCycleManager is missing. Cannot advance turn.",
                    LogCategory.Core,
                    this);
                return;
            }

            awaitingEndTurn = false;
            Phase = TurnPhase.Completed;

            int nextPlayer = turnCycleManager.Advance();
            turnStartedOutChannel?.RaiseEvent(new TurnStartedEvent(nextPlayer, 0));
        }

        private void OnTurnActionRequested(TurnActionRequest request)
        {
            if (request == null || request.player == null)
                return;

            int playerId = request.player.GetId();
            bool allowed = IsAllowed(playerId, request.action);

            turnActionResultChannel?.RaiseEvent(new TurnActionResult
            {
                playerId = playerId,
                action = request.action,
                allowed = allowed
            });

            if (!allowed) return;

            switch (request.action)
            {
                case TurnActionType.CompleteResolution:
                    CompleteResolution();
                    break;

                case TurnActionType.EndTurn:
                    CompleteTurnFlow();
                    break;
            }
        }

        private void CompleteResolution()
        {
            if (Phase != TurnPhase.AwaitingResolution)
                return;

            Phase = TurnPhase.Completed;
            awaitingEndTurn = true;
        }

        public void SetAwaitingEndTurn(bool awaiting)
        {
            awaitingEndTurn = awaiting;
        }

        public void TurnActionRequestTest(TurnActionRequest request)
        {
            OnTurnActionRequested(request);
        }

        private bool IsAllowed(int playerId, TurnActionType action)
        {

            if (playerId != ActivePlayer) return false;

            return action switch
            {
                TurnActionType.RollDice => Phase == TurnPhase.AwaitingRoll,
                // upgrade at any point in the player's turn
                TurnActionType.BuyProperty => Phase == TurnPhase.AwaitingResolution
                                                || (Phase == TurnPhase.Completed && awaitingEndTurn),
                TurnActionType.ModifyProperty => Phase == TurnPhase.AwaitingRoll
                                                || Phase == TurnPhase.AwaitingMovement
                                                || Phase == TurnPhase.AwaitingResolution
                                                || (Phase == TurnPhase.Completed && awaitingEndTurn),
                TurnActionType.CompleteResolution => Phase == TurnPhase.AwaitingResolution,
                TurnActionType.EndTurn => Phase == TurnPhase.Completed && awaitingEndTurn,
                _ => false
            };
        }
    }
}
