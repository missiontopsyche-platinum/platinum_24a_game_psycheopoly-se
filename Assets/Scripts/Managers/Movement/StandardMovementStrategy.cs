using System;
using UnityEngine;
using PsycheOpoly.Board;
using Logging;
using Logger = Logging.Logger;

namespace Assets.Scripts.Managers.Movement
{
    /// <summary>
    /// Default rule-driven movement strategy for Psyche-Opoly.
    /// Handles dice rolls, doubles, jail logic, and post-move resolution.
    /// </summary>
    public class StandardMovementStrategy : MonoBehaviour, IMovementStrategy
    {
        [Header("Dependencies")]
        [SerializeField] private BoardManager boardManager;
        //[SerializeField] private PlayerEventChannel currentPlayerChannel;

        [Header("Event Channels")]
        // Some channels are commented out as a work around for gamemanager to invoke methods directly.
        [SerializeField] private DiceRolledEventChannel diceRolledChannel;
        [SerializeField] private MovePlayerEventChannel movePlayerChannel;
        //[SerializeField] private BooleanEventChannel pieceMoveCompletedEventChannel;
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
            diceRolledChannel?.Subscribe(OnDiceRolled);
            //pieceMoveCompletedEventChannel?.Subscribe(OnPieceMoveCompleted);
            //currentPlayerChannel?.Subscribe(SetCurrentPlayer);
            turnStartedEventChannel?.Subscribe(OnTurnStarted);
        }


        private void OnDisable()
        {
            //diceRolledChannel?.Unsubscribe(OnDiceRolled);
            //pieceMoveCompletedEventChannel?.Unsubscribe(OnPieceMoveCompleted);
            //currentPlayerChannel?.Unsubscribe(SetCurrentPlayer);
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
            
            // this needs to get reset on every turn to ensure 'doubles count' from the last player doesn't impact the next.
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


                //TODO:
                // goToJailChannel, for now, is an IntEventChannel used to notify the JailManager 
                // (or any other subscribed system) that a player should be sent to jail.
                // since there is no JailManager logic is implemented, this call will do nothing 
                // unless a listener subscribes. this is just a fallback  to ensure the player is still 
                // sent to the Jail space (placheolder of index 10) so that the core behavior stil holds.
                if (goToJailChannel != null)
                    goToJailChannel.RaiseEvent(currentPlayer.GetId());
                else
                    boardManager.SetPlayerPosition(currentPlayer.GetId(), 10); //TODO: change to ACTUAL jail index

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

        public void OnPieceMoveCompleted(bool success)
        {
            if (!success || currentPlayer == null || normalMoveCompletedThisTurn)
                return;



            int playerId = currentPlayer.GetId();
            int currentPos = boardManager.GetPlayerPosition(playerId);

            // loop through last path (skip current space) and run each on-passed method (catch passing GO)
            foreach (int index in lastPath)
            {
                if (index == currentPos) continue;
                
                boardManager.GetSpace(index)?.OnPassed(currentPlayer);
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
                Logger.Debug("StandardMovementStrategy", "Doubles → allow re-roll", LogCategory.Gameplay, this);
                //this is where we can put a re-roll button...
                //recall also, we defined 3 doubles rolls is a jail sentence!
            }
            else
            {
                Logger.Debug("StandardMovementStrategy", "Turn complete.", LogCategory.Gameplay, this);
                doublesCount = 0;
            }

            spaceResolutionCompletedChannel?.RaiseEvent(true);

            normalMoveCompletedThisTurn = true;
        }

        //helprs

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
