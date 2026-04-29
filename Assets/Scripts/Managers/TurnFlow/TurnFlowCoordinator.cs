using System;
using System.Collections;
using Assets.Scripts.Managers.Jail;
using Assets.Scripts.Managers.TurnOrder;
using Logging;
using System.Collections.Generic;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Managers.TurnFlow
{
    /// <summary>
    /// centralized version for a turn progression controller.
    /// Listens to roll > movement > tile effects > resolution
    /// determines next player using TurnCycleManager + PlayerTurnState.
    /// </summary>
    public class TurnFlowCoordinator : MonoBehaviour
    {
        [FormerlySerializedAs("turnStartedInChannel")]
        [Header("Event Channel")]
        [SerializeField] private TurnStartedEventChannel turnStartedChannel;
        [SerializeField] private DiceRolledEventChannel diceRolledChannel;
        [SerializeField] private BooleanEventChannel spaceResolutionCompletedChannel;
        [SerializeField] private BooleanEventChannel pieceMoveCompletedChannel;
        [SerializeField] private TurnActionRequestEventChannel turnActionRequestChannel;
        [SerializeField] private TurnActionResultEventChannel turnActionResultChannel;

        [Header("Event Channels Out")]
        [SerializeField] private ActionResolvedEventChannel actionResolvedEventChannel;
        [SerializeField] private TurnStartedEventChannel turnStartedOutChannel;


        [Header("Dependencies")]
        [SerializeField] private TurnCycleManager turnCycleManager;

        public int ActivePlayer { get; private set; } = -1;
        public TurnPhase Phase { get; private set; } = TurnPhase.None;
        //these are needed for TFC to know if we are rolling for a jail escape
        private bool nextRollIsJailEscape = false;
        private Player jailEscapePlayer = null;
        // prevents double-advance in same turn
        private bool awaitingEndTurn = false;

        // fields to keep track of player states
        private readonly List<Player> players = new ();
        public bool IsGameOver { get; private set; }
        public static int LastWinningPlayerId { get; private set; } = -1;
        public static string LastWinningPlayerName { get; private set; } = string.Empty;
        private bool pendingCompleteResolution = false;
        private bool pendingEndTurnAfterResolution = false;

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
            turnStartedChannel?.RaiseEvent(new TurnStartedEvent(startingPlayer, 0));
        }

        private void OnEnable()
        {
            turnStartedChannel?.Subscribe(OnTurnStarted);
            diceRolledChannel?.Subscribe(OnDiceRolled);
            spaceResolutionCompletedChannel?.Subscribe(OnPieceMoveCompleted);
            turnActionRequestChannel?.Subscribe(OnTurnActionRequested);
        }

        private void OnDisable()
        {
            turnStartedChannel?.Unsubscribe(OnTurnStarted);
            diceRolledChannel?.Unsubscribe(OnDiceRolled);
            spaceResolutionCompletedChannel?.Unsubscribe(OnPieceMoveCompleted);
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
            pendingCompleteResolution = false;
            pendingEndTurnAfterResolution = false;
            nextRollIsJailEscape = false;
            jailEscapePlayer = null;

            Logging.Logger.Info("TurnFlowCoordinator.OnTurnStarted",
                $"New turn started: {data.turnNum} || Player: {players[ActivePlayer].GetPName()}",
                LogCategory.Core);

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
        // normal rolls advance movement, however, while flagged a jail roll, resolved through Jail
        // Utility before deciding whether the turn continues or ends
        private void OnDiceRolled(DiceRolledEvent diceEvent)
        {
            if (IsGameOver) return;
            if (Phase != TurnPhase.AwaitingRoll || diceEvent == null)
                return;

            //this is necessary to capture a proper jail-escape roll attempt
            if (nextRollIsJailEscape && jailEscapePlayer != null)
            {
                JailUtility.EscapeAttemptResult result =
                    JailUtility.AttemptEscape(jailEscapePlayer, diceEvent.dieOne, diceEvent.dieTwo);

                Logging.Logger.Info("TurnFlowCoordinator.OnDiceRolled",
                    $"{jailEscapePlayer.GetPName()} jail escape result: {result}.",
                    LogCategory.Gameplay,
                    this);

                nextRollIsJailEscape = false;

                switch (result)
                {
                    case JailUtility.EscapeAttemptResult.Escaped:
                        jailEscapePlayer.SetSuppressNextDoublesBonus(true);
                        Phase = TurnPhase.AwaitingMovement;
                        return;

                    case JailUtility.EscapeAttemptResult.ForcedExitPaid:
                        Phase = TurnPhase.AwaitingMovement;
                        return;

                    case JailUtility.EscapeAttemptResult.Failed:
                        Phase = TurnPhase.Completed;
                        awaitingEndTurn = true;
                        StartCoroutine(WaitForSecondsToRun(2, CompleteTurnFlow));
                        return;

                    case JailUtility.EscapeAttemptResult.ForcedExitBankrupt:
                        Phase = TurnPhase.Completed;
                        awaitingEndTurn = true;
                        StartCoroutine(WaitForSecondsToRun(2, CompleteTurnFlow));
                        return;
                }
                jailEscapePlayer = null;
            }
            Phase = TurnPhase.AwaitingMovement;
        }

        // using this to get rid of race conditions on failed dice rolls to escape jail
        private IEnumerator WaitForSecondsToRun(int seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action.Invoke();
        }


        // essenitally "complete resolution" part
        private void OnPieceMoveCompleted(bool success)
        {
            if (IsGameOver) return;
            if (!success) return;
            // guard in case this is called after movement resolution, to ensure
            // that we aren't accidentally setting the turn phase back and locking ourselves
            if (Phase == TurnPhase.Completed) return;

            Phase = TurnPhase.AwaitingResolution;

            Logging.Logger.Info("TurnFlowCoordinator.OnPieceMoveCompleted",
                    "Movement resolution completed. Awaiting turn resolution.",
                    LogCategory.Core,
                    this);

            actionResolvedEventChannel?.RaiseEvent(new ActionResolvedEvent(ActivePlayer));

            if (pendingCompleteResolution)
            {
                pendingCompleteResolution = false;
                CompleteResolution();

                if (pendingEndTurnAfterResolution)
                {
                    pendingEndTurnAfterResolution = false;
                    CompleteTurnFlow();
                }
            }
        }


        // decide if another turn is in order or if we can move onto next plater
        private void CompleteTurnFlow()
        {
            if (IsGameOver) return;

            if (Phase == TurnPhase.AwaitingMovement && pendingCompleteResolution)
            {
                pendingEndTurnAfterResolution = true;

                Logging.Logger.Info("TurnFlowCoordinator.CompleteTurnFlow",
                    "EndTurn requested before movement resolution finished. Deferring until resolution completes.",
                    LogCategory.Gameplay,
                    this);

                return;
            }

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
            turnStartedChannel?.RaiseEvent(new TurnStartedEvent(nextPlayer, 0));
        }

        private void OnTurnActionRequested(TurnActionRequest request)
        {
            if (IsGameOver) return;
            if (request == null || request.player == null)
                return;

            int playerId = request.player.GetId();
            bool allowed = IsAllowed(request.action, request.player);

            Logging.Logger.Info("TurnFlowCoordinator.OnTurnActionRequested",
                $"Turn action allowed: {allowed}" +
                $"\n\t{request.action} || {request.player.GetPName()}" +
                $"\n\tCurrent Phase: {Phase}",
                LogCategory.Gameplay);


            if (allowed)
            {
                switch (request.action)
                {
                    case TurnActionType.RollForJailEscape:
                        BeginJailEscapeRoll(request.player);
                        break;

                    case TurnActionType.CompleteResolution:
                        CompleteResolution();
                        break;

                    case TurnActionType.EndTurn:
                        CompleteTurnFlow();
                        break;
                }
            }

            turnActionResultChannel?.RaiseEvent(new TurnActionResult
            {
                playerId = playerId,
                action = request.action,
                allowed = allowed
            });
        }

        private void CompleteResolution()
        {
            if (IsGameOver) return;

            if (Phase == TurnPhase.AwaitingMovement)
            {
                pendingCompleteResolution = true;

                Logging.Logger.Info("TurnFlowCoordinator.CompleteResolution",
                    "CompleteResolution requested before movement resolution finished. Deferring.",
                    LogCategory.Gameplay,
                    this);

                return;
            }


            if (Phase == TurnPhase.Completed)
                return;

            if (Phase != TurnPhase.AwaitingResolution)
                return;

            Phase = TurnPhase.Completed;
            awaitingEndTurn = true;

            Logging.Logger.Info("TurnFlowCoordinator.CompleteResolution",
                "Turn phase transitioned to Completed. Can end turn.",
                LogCategory.Gameplay,
                this);

        }

        //unique roll for jail escape
        private void BeginJailEscapeRoll(Player player)
        {
            if (player == null)
                return;

            nextRollIsJailEscape = true;
            jailEscapePlayer = player;

            Logging.Logger.Info("TurnFlowCoordinator.BeginJailEscapeRoll",
                $"{player.GetPName()} is attempting to escape jail.",
                LogCategory.Gameplay,
                this);
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
            //if (player.GetId() != ActivePlayer) return false;

            //sends to the roll for escape turn action (since it's a unique dice roll)
            if (player != null && player.IsInJail())
            {
                return action switch
                {
                    TurnActionType.RollForJailEscape => Phase == TurnPhase.AwaitingRoll,

                    TurnActionType.CompleteResolution => Phase == TurnPhase.AwaitingMovement
                                                         || Phase == TurnPhase.AwaitingResolution
                                                         || Phase == TurnPhase.Completed,

                    TurnActionType.EndTurn => (Phase == TurnPhase.Completed && awaitingEndTurn)
                                              || (Phase == TurnPhase.AwaitingMovement && pendingCompleteResolution),

                    _ => false
                };
        }

            return action switch
            {
                TurnActionType.RollDice => Phase == TurnPhase.AwaitingRoll,
                TurnActionType.BuyProperty => Phase == TurnPhase.AwaitingMovement
                                              || Phase == TurnPhase.AwaitingResolution
                                              || (Phase == TurnPhase.Completed && awaitingEndTurn),
                TurnActionType.ModifyProperty => Phase == TurnPhase.AwaitingRoll
                                                 || Phase == TurnPhase.AwaitingMovement
                                                 || Phase == TurnPhase.AwaitingResolution
                                                 || (Phase == TurnPhase.Completed && awaitingEndTurn),
                TurnActionType.CompleteResolution => Phase == TurnPhase.AwaitingMovement 
                                                    || Phase == TurnPhase.AwaitingResolution
                                                    || Phase == TurnPhase.Completed,
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

    if (GameManager.instance != null)
    {
        GameManager.instance.SetWinner(winner);
        GameManager.instance.EndGame();
    }
    else
    {
        Logging.Logger.Warn("TurnFlowCoordinator.TriggerGameEnd",
            "GameManager instance was null. Could not end game through GameManager.",
            LogCategory.Core,
            this);
    }
}

private Player GetPlayerById(int playerId)
{
    if (playerId < 0 || playerId >= players.Count)
        return null;

    return players[playerId];
}
    }
}
