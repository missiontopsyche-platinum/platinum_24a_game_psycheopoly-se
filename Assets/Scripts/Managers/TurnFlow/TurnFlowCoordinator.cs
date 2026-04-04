using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Managers.TurnOrder;
using Logging;
using System.Collections.Generic;
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

        // fields to keep track of player states
        private readonly List<Player> players;
        public bool IsGameOver { get; private set; }
        public static int LastWinningPlayerId { get; private set; } = -1;
        public static string LastWinningPlayerName { get; private set; } = string.Empty;

        public void Initialize(TurnCycleManager tcm, List<Player> gamePlayers)
        {
            if (turnCycleManager == null)
            {
                turnCycleManager = tcm;
            }

            players.Clear();

            if (gamePlayers != null)
            {
                players.AddRange(gamePlayers);
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

            IsGameOver = false;
            LastWinningPlayerId = -1;
            LastWinningPlayerName = string.Empty;
            int startingPlayer = turnCycleManager.CurrentPlayerIndex;
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
            HandleBankruptcyPruningAtTurnStart();
            if (IsGameOver)
                return;

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
            if (IsGameOver) return;
            if (Phase != TurnPhase.AwaitingRoll) return;
            Phase = TurnPhase.AwaitingMovement;
        }


        // essenitally "complete resolution" part
        private void OnPieceMoveCompleted(bool success)
        {
            if (IsGameOver) return;
            if (!success) return;
            if (Phase != TurnPhase.AwaitingMovement) return;

            Phase = TurnPhase.AwaitingResolution;

            actionResolvedEventChannel?.RaiseEvent(new ActionResolvedEvent(ActivePlayer));
        }


        // decide if another turn is in order or if we can move onto next plater
        private void CompleteTurnFlow()
        {
            if (IsGameOver) return;
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
            if (IsGameOver) return;
            if (request == null || request.player == null)
                return;

            int playerId = request.player.GetId();
            bool allowed = IsAllowed(request.action, request.player);

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
            if (IsGameOver) return;
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

        private bool IsAllowed(TurnActionType action, Player player)
        {
            if (IsGameOver) return false;
            if (player.GetId() != ActivePlayer) return false;

            if (player != null && player.IsInJail())
                return action == TurnActionType.EndTurn;
            

            return action switch
            {
                TurnActionType.RollDice => Phase == TurnPhase.AwaitingRoll,
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

        private void HandleBankruptcyPruningAtTurnStart()
        {
            if (turnCycleManager == null || players.Count == 0)
                return;

            turnCycleManager.PruneBankruptPlayers(players);

            int activePlayers = turnCycleManager.GetActivePlayerCount();

            if (activePlayers <= 1)
            {
                int winnerIndex = turnCycleManager.GetLastRemainingPlayerIndex();
                Player winner = GetPlayerById(winnerIndex);
                TriggerGameEnd(winner);
                return;
            }

            if (turnCycleManager.IsPlayerEliminated(ActivePlayer))
            {
                Logging.Logger.Info("TurnFlowCoordinator.HandleBankruptcyPruningAtTurnStart",
                    $"Player {ActivePlayer} was pruned before taking their turn. Advancing to next player.",
                    LogCategory.Gameplay,
                    this);

                int nextPlayer = turnCycleManager.Advance();
                turnStartedOutChannel?.RaiseEvent(new TurnStartedEvent(nextPlayer, 0));
            }
        }

        private void TriggerGameEnd(Player winner)
        {
            if (IsGameOver)
                return;

            IsGameOver = true;
            Phase = TurnPhase.None;
            awaitingEndTurn = false;
            ActivePlayer = -1;

            LastWinningPlayerId = winner != null ? winner.GetId() : -1;
            LastWinningPlayerName = winner != null ? winner.GetPName() : "No Winner";

            Logging.Logger.Info("TurnFlowCoordinator.TriggerGameEnd",
                $"Game over. Winner: {LastWinningPlayerName}",
                LogCategory.Gameplay,
                this);
        }

        private Player GetPlayerById(int playerId)
        {
            if (playerId < 0 || playerId >= players.Count)
                return null;

            return players[playerId];
        }
    }
}
