using UnityEngine;
using Assets.Scripts.Managers.TurnOrder;
using Assets.Scripts.Managers.Movement;

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

        [Header("Event Channels Out")]
        [SerializeField] private ActionResolvedEventChannel actionResolvedEventChannel;
        [SerializeField] private TurnStartedEventChannel turnStartedOutChannel;

        [Header("Dependencies")]
        [SerializeField] private TurnCycleManager turnCycleManager;
        [SerializeField] private PlayerManager playerManager;

        private TurnPhase phase = TurnPhase.None;
        private int activePlayer = -1;

        private void Awake()
        {
            if (!turnCycleManager)
                turnCycleManager = FindFirstObjectByType<TurnCycleManager>();

            if (!playerManager)
                playerManager = FindFirstObjectByType<PlayerManager>();

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
        }

        private void OnDisable()
        {
            turnStartedInChannel?.Unsubscribe(OnTurnStarted);
            diceRolledChannel?.Unsubscribe(OnDiceRolled);
            pieceMoveCompletedChannel?.Unsubscribe(OnPieceMoveCompleted);
            turnEndedChannel?.Unsubscribe(OnTurnEnded);
        }

        // a new turn's started, reset state, wait for new roll
        private void OnTurnStarted(TurnStartedEvent data)
        {
            activePlayer = data.playerId;
            phase = TurnPhase.AwaitingRoll;
        }

        private void OnTurnEnded(bool ended)
        {
            if (!ended) return;
            CompleteTurnFlow();
        }

        // after dice roll, wait for movement to contine
        private void OnDiceRolled(DiceRolledEvent diceEvent)
        {
            if (phase != TurnPhase.AwaitingRoll) return;
            phase = TurnPhase.AwaitingMovement;
        }


        // essenitally "complete resolution" part
        private void OnPieceMoveCompleted(bool success)
        {
            if (!success) return;
            if (phase != TurnPhase.AwaitingMovement) return;


            phase = TurnPhase.AwaitingResolution;

            // OnLanded() has already run at this point.
            // No tile effect system fires a "done" event, so we emit our own.
            actionResolvedEventChannel?.RaiseEvent(new ActionResolvedEvent(activePlayer));

            CompleteTurnFlow();
        }


        // decide if another turn is in order or if we can move onto next plater
        private void CompleteTurnFlow()
        {
            if (turnCycleManager == null)
                return;

            phase = TurnPhase.Completed;

            int nextPlayer = turnCycleManager.Advance();
            turnStartedOutChannel?.RaiseEvent(new TurnStartedEvent(nextPlayer, 0));
        }
    }

}
