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

        [Header("Event Channels")]
        [SerializeField] private MovePlayerEventChannel movePlayerChannel;
        [SerializeField] private IntEventChannel goToJailChannel;
        [SerializeField] private BooleanEventChannel spaceResolutionCompletedChannel;

        private int doublesCount = 0;
        private Player currentPlayer;
        private int[] lastPath;
        private bool normalMoveCompletedThisTurn = false;
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
        }

        //resets movement state for the active players turn, caller responsible for passing correct player
        public void BeginTurn(Player player)
        {
            if (player == null)
            {
                Logger.Error("StandardMovementStrategy.BeginTurn", "Cannot begin movement turn with null player.",
                    LogCategory.Gameplay, this);
                return;
            }

            bool samePlayerContinuing = currentPlayer != null && currentPlayer.GetId() == player.GetId();

            currentPlayer = player;
            lastPath = null;
            normalMoveCompletedThisTurn = false;
            movementInProgress = false;

            if (!samePlayerContinuing)
                doublesCount = 0;
        }

        //executes current players movement using the DiceRolledEvent as payload object containing die values
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
                    goToJailChannel.RaiseEvent(currentPlayer.GetId());
                else
                    boardManager.SetPlayerPosition(currentPlayer.GetId(), 10);

                return;
            }

            int playerId = currentPlayer.GetId();
            int startIndex = boardManager.GetPlayerPosition(playerId);
            int endIndex = NormalizeIndex(startIndex + total, boardManager.boardSize);
            int[] pathIndices = BuildPathIndices(startIndex, total, boardManager.boardSize);

            Logger.Debug("StandardMovementStrategy.ExecuteRollMovement",
                $"Player {playerId} rolled {die1}+{die2}={total} moving from {startIndex} to {endIndex}.",
                LogCategory.Gameplay, this);

            lastPath = pathIndices;
            movementInProgress = true;

            normalMoveCompletedThisTurn = false;

            MovePlayerEvent moveEvent = new MovePlayerEvent(playerId, total, pathIndices);
            movePlayerChannel?.RaiseEvent(moveEvent);


        }

        //resolves passed spaces and landed spaces, caller will invoke after movement completion signals
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
                    "Doubles rolled. Extra roll remains available to current flow.",
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


        // returns true for when doubles are rolled and the player has an extra roll
        public bool HasExtraRollAvailable()
        {
            return currentPlayer != null && doublesCount > 0 && !movementInProgress;
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
