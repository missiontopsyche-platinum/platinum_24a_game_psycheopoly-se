using UnityEngine;
using Assets.Scripts.Managers.TurnOrder;

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
        [SerializeField] private BooleanEventChannel turnEndedChannel;
        [SerializeField] private TurnActionRequestEventChannel turnActionRequestChannel;
        [SerializeField] private TurnActionResultEventChannel turnActionResultChannel;

        [Header("Event Channels Out")]
        [SerializeField] private ActionResolvedEventChannel actionResolvedEventChannel;
        [SerializeField] private TurnStartedEventChannel turnStartedOutChannel;

        private TurnCycleManager turnCycleManager;

        public TurnPhase Phase { get; private set; } = TurnPhase.None;
        public int ActivePlayer { get; private set; } = -1;
        // prevents double-advance in same turn
        private bool awaitingEndTurn = false;

        private void Awake()
        {
            // in the future we should be passing this in from GameManager or something.
            turnCycleManager = new TurnCycleManager(4);

            //fallback for editmode tests
            var gm = FindFirstObjectByType<GameManager>();
            if (!turnStartedInChannel && gm) turnStartedInChannel = gm.turnStartedChannel;
            if (!turnEndedChannel && gm) turnEndedChannel = gm.turnEndedChannel;
        }



        private void OnEnable()
        {
            turnStartedInChannel?.Subscribe(OnTurnStarted);
            diceRolledChannel?.Subscribe(OnDiceRolled);
            pieceMoveCompletedChannel?.Subscribe(OnPieceMoveCompleted);
            turnEndedChannel?.Subscribe(OnTurnEnded);
            turnActionRequestChannel?.Subscribe(OnTurnActionRequested);
        }

        private void OnDisable()
        {
            turnStartedInChannel?.Unsubscribe(OnTurnStarted);
            diceRolledChannel?.Unsubscribe(OnDiceRolled);
            pieceMoveCompletedChannel?.Unsubscribe(OnPieceMoveCompleted);
            turnEndedChannel?.Unsubscribe(OnTurnEnded);
            turnActionRequestChannel?.Unsubscribe(OnTurnActionRequested);
        }

        // a new turn's started, reset state, wait for new roll
        private void OnTurnStarted(TurnStartedEvent data)
        {
            if (turnCycleManager == null) 
                turnCycleManager = FindFirstObjectByType<GameManager>().turnCycleManager;
            
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

        private void OnTurnEnded(bool ended)
        {
            if (!ended) return;
            CompleteTurnFlow();
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
            Phase = TurnPhase.Completed;

            awaitingEndTurn = true;
            // OnLanded() has already run at this point.
            // No tile effect system fires a "done" event, so we emit our own.
            actionResolvedEventChannel?.RaiseEvent(new ActionResolvedEvent(ActivePlayer));
        }


        // decide if another turn is in order or if we can move onto next plater
        private void CompleteTurnFlow()
        {
            if (!awaitingEndTurn) return;
            if (turnCycleManager == null)
                return;
            awaitingEndTurn = false;

            Phase = TurnPhase.Completed;

            int nextPlayer = turnCycleManager.Advance();
            turnStartedOutChannel?.RaiseEvent(new TurnStartedEvent(nextPlayer, 0));
        }

        public void OnTurnActionRequested(TurnActionRequest request)
        {
            bool allowed = IsAllowed(request.player.GetId(), request.action);
            turnActionResultChannel.RaiseEvent(new TurnActionResult
            {
                playerId = request.player.GetId(),
                action = request.action,
                allowed = allowed
            });
            if (allowed && request.action == TurnActionType.EndTurn)
                CompleteTurnFlow();
            
        }

        public void SetAwaitingEndTurn(bool awaiting)
        {
            awaitingEndTurn = awaiting;
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
                TurnActionType.EndTurn => Phase == TurnPhase.Completed && awaitingEndTurn,
                _ => false
            };
        }
    }
}
