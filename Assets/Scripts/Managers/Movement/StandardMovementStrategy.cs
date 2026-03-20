using UnityEngine;
using PsycheOpoly.Board;
using Logging;
using Logger = Logging.Logger;

namespace Assets.Scripts.Managers.Movement
{
    /// <summary>
    /// Direct movement component 
    /// Called by owning gameplay flow after a dice roll has been made for the active player
    /// This component interpets the roll into movement, applied movement-specific rules such
    /// as doubles, triple doubles, raises movement request, and resolves passed/landed space
    /// behavior after the piece finishes moving.
    /// </summary>
    public class StandardMovementStrategy : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private BoardManager boardManager;

        [Header("Event Channels")]
        [SerializeField] private MovePlayerEventChannel movePlayerChannel;
        [SerializeField] private TurnStartedEventChannel turnStartedEventChannel;
        [SerializeField] private IntEventChannel goToJailChannel;
        [SerializeField] private BooleanEventChannel spaceResolutionCompletedChannel;

        private int doublesCount = 0;
        private Player currentPlayer;
        private PlayerManager playerManager;
        private int[] lastPath;
        private bool normalMoveCompletedThisTurn = false;

        private void OnEnable()
        {
            turnStartedEventChannel?.Subscribe(OnTurnStarted);
        }


        private void OnDisable()
        {
            turnStartedEventChannel?.Unsubscribe(OnTurnStarted);

        }

        void Start()
        {
            if (boardManager == null)
            {
                boardManager = FindAnyObjectByType<BoardManager>();
                if (boardManager == null)
                    Logger.Error(
                        "StandardMovementStrategy.Start",
                        "Board Manager component not found in scene!",
                        LogCategory.Core, this);
            }
        }

        /// <summary>
        /// sets/updates current active player (injected via GameManager or TurnSystem).
        /// </summary>
        private void SetCurrentPlayer(Player p)
        {
            currentPlayer = p;
            normalMoveCompletedThisTurn = false;
        }

        private void Awake()
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
            if (playerManager == null)
            {
                Logger.Error("StandardMovementStrategy.Awake", "PlayerManager not found in scene.",
                    LogCategory.Gameplay, this);
            }

        }


        private void OnTurnStarted(TurnStartedEvent turnData)
        {
            if (playerManager == null)
                return;


            Player p = playerManager.GetPlayer(turnData.playerId);

            if (p == null)
            {
                Logger.Error("StandardMovementStrategy.OnTurnStarted", $"Invalid playerId {turnData.playerId}",
                    LogCategory.Gameplay, this);
                return;
            }

            SetCurrentPlayer(p);

            //reset doubles @ start of players turn so prev. players state isn't carried
            doublesCount = 0;
        }


        public void OnDiceRolled(DiceRolledEvent diceEvent)
        {
            if (currentPlayer == null)
            {
                Logger.Warn("StandardMovementStrategy.OnDiceRolled",
                    "No current player set.", LogCategory.Gameplay, this);
                return;
            }

            int die1 = diceEvent.dieOne;
            int die2 = diceEvent.dieTwo;
            int total = diceEvent.totalRoll;

            bool isDouble = die1 == die2;
            if (isDouble) doublesCount++;
            else doublesCount = 0;

            // Triple doubles → Go To Jail
            if (doublesCount >= 3)
            {
                Logger.Info("StandardMovementStrategy",
                    "Triple doubles rolled → Sending player to Jail",
                    LogCategory.Gameplay, this);

                doublesCount = 0;

                if (goToJailChannel != null)
                    goToJailChannel.RaiseEvent(currentPlayer.GetId());
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



            MovePlayerEvent moveEvent = new MovePlayerEvent(currentPlayer.GetId(), total, pathIndices);
            movePlayerChannel?.RaiseEvent(moveEvent);
            
            // store to run "OnPassed" on spaces
            lastPath = pathIndices;
        }

        public void OnPieceMoveCompleted(bool success)
        {
            if (!success || currentPlayer == null || normalMoveCompletedThisTurn)
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

            Logger.Info("StandardMovementStrategy",
                $"Player {playerId} landed on {landed?.GetType().Name ?? "Unknown"} (Index {currentPos})",
                LogCategory.Gameplay, this);

            //landing effect for the space
            landed?.OnLanded(currentPlayer);

            //handles the doubles roll logic
            if (doublesCount > 0)
            {
                Logger.Debug("StandardMovementStrategy", "Doubles → allow re-roll", 
                    LogCategory.Gameplay, this);
            }
            else
            {
                Logger.Debug("StandardMovementStrategy", "Turn complete.", LogCategory.Gameplay, this);
                doublesCount = 0;
            }

            spaceResolutionCompletedChannel?.RaiseEvent(true);
            normalMoveCompletedThisTurn = true;
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
