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

        [Header("Dependencies")]
        [SerializeField] private TurnCycleManager turnCycleManager;

        public TurnPhase Phase { get; private set; } = TurnPhase.None;
        public int ActivePlayer { get; private set; } = -1;

        private void Awake()
        {
            if (!turnCycleManager)
                turnCycleManager = FindFirstObjectByType<TurnCycleManager>();

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
            ActivePlayer = data.playerId;
            Phase = TurnPhase.AwaitingRoll;
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

            // OnLanded() has already run at this point.
            // No tile effect system fires a "done" event, so we emit our own.
            actionResolvedEventChannel?.RaiseEvent(new ActionResolvedEvent(ActivePlayer));

            CompleteTurnFlow();
        }


        // decide if another turn is in order or if we can move onto next plater
        private void CompleteTurnFlow()
        {
            if (turnCycleManager == null)
                return;

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
        }

        private bool IsAllowed(int playerId, TurnActionType action)
        {

            if (playerId != ActivePlayer) return false;

            return action switch
            {
                TurnActionType.RollDice => Phase == TurnPhase.AwaitingRoll,
                TurnActionType.BuyProperty => Phase == TurnPhase.AwaitingResolution,
                // upgrade at any point in the player's turn
                TurnActionType.UpgradeProperty => Phase == TurnPhase.None
                                                || Phase == TurnPhase.AwaitingRoll
                                                || Phase == TurnPhase.AwaitingMovement
                                                || Phase == TurnPhase.AwaitingResolution 
                                                || Phase == TurnPhase.Completed,
                TurnActionType.EndTurn => Phase == TurnPhase.Completed,
                _ => false
            };
        }
    }
}
