using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using UnityEngine;
using PsycheOpoly.Board;
using Logging;
using Logger = Logging.Logger;

namespace Assets.Scripts.Managers.Movement
{
    /// <summary>
    /// Concrete movement component used by the game flow.
    /// Called directly by the flow owner after an active player has been set for the turn.
    /// Interprets dice roll payload data into movement, applies movement specific rules
    /// such as doubles &  triple doubles, raises board movement requests, and resolves
    /// passed and landed space behavior after movement completes.
    /// </summary>
    public class StandardMovementStrategy : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private PlayerManager playerManager;

        [Header("Event Channels")]
        [SerializeField] private MovePlayerEventChannel movePlayerChannel;
        [SerializeField] private JailStateChangedEventChannel goToJailChannel;
        [SerializeField] private BooleanEventChannel spaceResolutionCompletedChannel;
        [SerializeField] private TurnStartedEventChannel turnStartedEventChannel;
        [SerializeField] private BooleanEventChannel pieceMoveCompletedEventChannel;

        private int doublesCount = 0;
        private Player currentPlayer;
        private int[] lastPath;
        private bool normalMoveCompletedThisTurn = false;

        private void OnEnable()
        {
            turnStartedEventChannel?.Subscribe(OnTurnStarted);
            pieceMoveCompletedEventChannel?.Subscribe(ResolveCompletedMovement);
        }


        private void OnDisable()
        {
            turnStartedEventChannel?.Unsubscribe(OnTurnStarted);
            pieceMoveCompletedEventChannel?.Unsubscribe(ResolveCompletedMovement);
        }

        private bool movementInProgress = false;

        private void Awake()
        {
            if (boardManager == null)
            {
                boardManager = FindFirstObjectByType<BoardManager>();

                if (boardManager == null)
                {
                    Logger.Error("StandardMovementStrategy.Awake", "BoardManager not found in scene.",
                        LogCategory.Core, this);
                }

            }

            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();

                if (playerManager == null)
                {
                    Logger.Error("StandardMovementStrategy.Awake", "PlayerManager not found in scene.",
                        LogCategory.Core, this);
                }
            }

        }


        /// <summary>
        /// Handles turn-start sync for Movement by resolving the active player from
        ///the incoming turn data & preserving doubles state when the same player continues
        /// </summary>
        private void OnTurnStarted(TurnStartedEvent turnData)
        {
            if (turnData == null)
            {
                Logger.Warn("StandardMovementStrategy.OnTurnStarted", "TurnStartedEvent was null.",
                    LogCategory.Gameplay, this);
                return;
            }

            if (playerManager == null)
            {
                Logger.Error("StandardMovementStrategy.OnTurnStarted", "PlayerManager reference is missing.",
                    LogCategory.Gameplay, this);
                return;
            }

            Player nextPlayer = playerManager.GetPlayer(turnData.playerId);

            if (nextPlayer == null)
            {
                Logger.Error("StandardMovementStrategy.OnTurnStarted",
                    $"Could not resolve player for id {turnData.playerId}.",
                    LogCategory.Gameplay, this);
                return;
            }

            bool samePlayerContinuing =
                currentPlayer != null && currentPlayer.GetId() == turnData.playerId;


            currentPlayer = nextPlayer;
            lastPath = null;
            normalMoveCompletedThisTurn = false;
            movementInProgress = false;

            if (!samePlayerContinuing)
                doublesCount = 0;
        }

        // called by dice manager, begins movement
        public void ExecuteRollMovement(DiceRolledEvent diceRoll)
        {
            if (currentPlayer == null)
            {
                Logger.Warn("StandardMovementStrategy.ExecuteRollMovement", "No active player set for movement.",
                    LogCategory.Gameplay, this);
                return;
            }

            if (boardManager == null)
            {
                Logger.Error("StandardMovementStrategy.ExecuteRollMovement", "BoardManager is not assigned.",
                    LogCategory.Core, this);
                return;
            }

            if (movementInProgress)
            {
                Logger.Warn("StandardMovementStrategy.ExecuteRollMovement", 
                    $"Movement already in progress for player {currentPlayer.GetId()}.",
                    LogCategory.Gameplay, this);
                return;
            }

            int die1 = diceRoll.dieOne;
            int die2 = diceRoll.dieTwo;
            int total = diceRoll.totalRoll;

            bool isDouble = die1 == die2;
            if (isDouble) doublesCount++;
            else doublesCount = 0;

            //triple doubles > Go To Jail
            if (doublesCount >= 3)
            {
                Logger.Info(
                    "StandardMovementStrategy.ExecuteRollMovement",
                    $"Player {currentPlayer.GetId()} rolled triple doubles and is sent to jail.",
                    LogCategory.Gameplay,
                    this);

                doublesCount = 0;
                movementInProgress = false;
                lastPath = null;

                if (goToJailChannel != null)
                    goToJailChannel.RaiseEvent(new JailStateChangedEvent(currentPlayer, true, 0));
                else
                    boardManager.SetPlayerPosition(currentPlayer.GetId(), 10);

                return;
            }

            // Standard movement
            int startIndex = boardManager.GetPlayerPosition(currentPlayer.GetId());
            int endIndex = NormalizeIndex(startIndex + total, boardManager.boardSize);

            int[] pathIndices = BuildPathIndices(startIndex, total, boardManager.boardSize);

            Logger.Debug("StandardMovementStrategy",
                $"Player {currentPlayer.GetId()} rolled {die1}+{die2}={total} moving from {startIndex}→{endIndex}",
                LogCategory.Gameplay, this);


            movePlayerChannel?.RaiseEvent(new MovePlayerEvent(currentPlayer.GetId(), total, pathIndices));
            
            // store to run "OnPassed" on spaces
            lastPath = pathIndices;
        }

        //resolves passed spaces and landed spaces, caller will invoke after movement completion signals
        // called by PieceMoveCompleted event fired by Piece at the end of the movement animation
        public void ResolveCompletedMovement(bool movementSucceeded)
        {
            if (!movementSucceeded)
                return;

            if (currentPlayer == null)
            {
                Logger.Warn("StandardMovementStrategy.ResolveCompletedMovement",
                    "No active player set when resolving movement completion.",
                    LogCategory.Gameplay, this);
                return;
            }

            if (!movementInProgress)
            {
                Logger.Warn("StandardMovementStrategy.ResolveCompletedMovement",
                    "ResolveCompletedMovement called with no movement in progress.",
                    LogCategory.Gameplay, this);
                return;
            }

            if (normalMoveCompletedThisTurn)
                return;

            int playerId = currentPlayer.GetId();

            int currentPos = boardManager.GetPlayerPosition(playerId);

            if (lastPath != null && lastPath.Length > 0)
            {
                foreach (int index in lastPath)
                {
                    if (index == currentPos)
                        continue;

                    boardManager.GetSpace(index)?.OnPassed(currentPlayer);
                }
            }

            SpaceData landed = boardManager.GetSpace(currentPos);

            Logger.Info("StandardMovementStrategy.ResolveCompletedMovement",
                $"Player {playerId} landed on {landed?.GetType().Name ?? "Unknown"} at index {currentPos}.",
                LogCategory.Gameplay,this);

            
            landed?.OnLanded(currentPlayer);

            if (doublesCount > 0)
            {
                Logger.Debug("StandardMovementStrategy.ResolveCompletedMovement",
                    "Doubles rolled. Current player still has an extra turn available.",
                    LogCategory.Gameplay,this);
            }
            else
            {
                Logger.Debug("StandardMovementStrategy.ResolveCompletedMovement",
                    "Movement and space resolution complete.",
                    LogCategory.Gameplay,this);
            }

            movementInProgress = false;
            normalMoveCompletedThisTurn = true;
            spaceResolutionCompletedChannel?.RaiseEvent(true);
        }

        private static int NormalizeIndex(int raw, int boardSize)
        {
            if (boardSize <= 0) return 0;
            int idx = raw % boardSize;
            return idx < 0 ? idx + boardSize : idx;
        }

        private static int[] BuildPathIndices(int startIndex, int spacesToMove, int boardSize)
        {
            int[] path = new int[spacesToMove];
            for (int i = 0; i < spacesToMove; i++)
            {
                path[i] = NormalizeIndex(startIndex + i + 1, boardSize);
            }

            return path;
        }

    }
}
