using System;
using System.Collections.Generic;
using UnityEngine;
using Logging;
using Logger = Logging.Logger;

namespace PsycheOpoly.Board
{
    [ExecuteAlways]
    public class BoardManager : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int defaultBoardSize = 40;

        [Header("Render Components")]
        [SerializeField] public BoardRenderer boardRenderer;

        [Header("Event Channels")]
        [SerializeField] public PlayerEventChannel playerAddedChannel;
        [SerializeField] public PlayerMovedEventChannel playerMovedChannel;
        [SerializeField] public MovePlayerEventChannel movePlayerChannel;
        [SerializeField] public IntEventChannel passedGoChannel;
        [SerializeField] public MoveToSpaceEventChannel moveToSpaceEventChannel;

        [Header("Board Spaces")]
        [SerializeField] public BoardSpaceContainer boardSpaceContainer;
        private SpaceData[] boardSpaces;
        
        //For Testing purposes
        public int boardSize => boardSpaces?.Length ?? 0;

        //Task 85 player position dictionary
        private readonly Dictionary<int, int> playerPositions = new Dictionary<int, int>();

        //Task 88 subscribe and Task 89 unsubscribe 
        private bool subscribed;

        private void Awake()
        {
            EnsureSubscribed();

            Logger.Initialize(LogSettings.Current());
        }

        private void OnEnable() => EnsureSubscribed();

        private void Start()
        {
            InitializeBoard();
            movePlayerChannel?.Subscribe(MovePlayer);
        }

        private void OnDisable() => EnsureUnsubscribed();

        private void OnDestroy() => EnsureUnsubscribed();


        private void EnsureSubscribed()
        {
            Logger.Info("BoardManager.EnsureSubscribed",
                "BoardManager is subscribing.",
                LogCategory.Core, this);
            if (subscribed) return;
            if (!this) return;
            playerAddedChannel?.Subscribe(AddPlayer);
            movePlayerChannel?.Subscribe(MovePlayer);
            moveToSpaceEventChannel?.Subscribe(OnMoveToSpaceEvent);
            subscribed = true;
        }

        private void EnsureUnsubscribed()
        {
            Logger.Info("BoardManager.EnsureUnsubscribed",
                "BoardManager is unsubscribing.",
                LogCategory.Core, this);
            if (!subscribed) return;
            playerAddedChannel?.Unsubscribe(AddPlayer);
            movePlayerChannel?.Unsubscribe(MovePlayer);
            moveToSpaceEventChannel?.Unsubscribe(OnMoveToSpaceEvent);
            subscribed = false;
        }

        // adds a new player. Something weird was happening with the dict and was causing players
        // to "start" at arbitrary indices rippling down to incorrect positioning for the renderer.
        private void AddPlayer(Player player)
        {   
            // This was changed to try add for debuggin US421
            playerPositions.TryAdd(player.GetId(), 0);
            Logger.Debug("AddPlayer",
                $"Player {player.GetId()} added at position {playerPositions[player.GetId()]}",
                LogCategory.Gameplay, this);
        }

        //Task 82 create InitializeBoard method 
        public void InitializeBoard()
        {
            // if null (for tests), pull default board from resources path.
            if (boardSpaceContainer == null)
                boardSpaceContainer = Resources.Load<BoardSpaceContainer>("Spaces/DefaultBoardContainer");

            boardSpaces = boardSpaceContainer.GetSpaces();
            
            if (!VerifyBoard())
                return;
            
            boardRenderer?.GenerateBoard(boardSpaces);
            
            EnsureSubscribed();
        }

        private bool VerifyBoard()
        {
            for (int i = 0; i < boardSpaces.Length; i++)
            {
                // grab the numbers prefix that is expected for our objects
                String spaceNumber = boardSpaces[i].ToString().Substring(0, 2);
                // formats the current index to a string with leading 0s, to match naming conventions
                String currentIndex = $"{i:00}";
                
                // if the prefix is as expected, we continue, or we break early with an error.
                if (spaceNumber.Equals(currentIndex))
                    continue;
                
                Logger.Error("BoardManager.VerifyBoard",
                    $"Board Array is incorrect for index {currentIndex}. Ensure SpaceData objects are in correct order.",
                    LogCategory.Core, this);
                return false;
            }
            
            Logger.Info("BoardManager.VerifyBoard",
                "Board has been verified successfully!",
                LogCategory.Core, this);

            return true;
        }

        //Task 84 which is GetSpace(int) with a wrap around
        public SpaceData GetSpace(int index)
        {
            if (boardSpaces == null || boardSpaces.Length == 0)
            {
                Logging.Logger.Error("BoardManager.GetSpace",
                    "Board not initialized, call InitializeBoard()",
                    LogCategory.Gameplay,
                    this);
                throw new InvalidOperationException("Board not initialized, call InitializeBoard()");
            }
            return boardSpaces[NormalizeIndex(index)];
        }

        /// <summary>
        /// Used to teleport a player to a new space
        /// This does not use dice rolls. 
        /// Should be used for events like "Go To Jail" 
        /// that require a player to not move across other spaces
        /// Does not check for backwards movement
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="spaceIndex"></param>
        public void SetPlayerPosition(int playerID, int spaceIndex)
        {
            EnsureBoard();
            if (spaceIndex < 0 || spaceIndex >= boardSpaces.Length)
            {
                throw new ArgumentOutOfRangeException("Space Index not in valid range");
            }
            else
            {
                playerPositions[playerID] = NormalizeIndex(spaceIndex);
            }
        }

        /// <summary>
        /// Returns the position of the specified player
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public int GetPlayerPosition(int playerID)
        {
            EnsureBoard();
            int currentPos = playerPositions.TryGetValue(playerID, out var idx) ? idx : 0;
            return currentPos;
        }

        /// <summary>
        /// Moves the player based on the movePlayerEvent being called
        /// Takes in a MovePlayerEvent object
        /// Then moves the specified player by the amount specified.
        /// Verifies if the user passes go.
        /// </summary>
        /// <param name="mpe"></param>
        public void MovePlayer(MovePlayerEvent mpe)
        {
            EnsureBoard();
            int previous = GetPlayerPosition(mpe.id);
            int spaces = mpe.spacesToMove;
            int next = NormalizeIndex(previous + spaces);

            //more robust path building code
            int steps = Mathf.Abs(spaces);
            int[] path = new int[steps];

            if (spaces > 0)
            {
                //moving forward
                for (int i = 0; i < steps; i++)
                {
                    path[i] = NormalizeIndex(previous + (i + 1));
                }
            }
            else if (spaces < 0)
            {
                //moving backward
                for (int i = 0; i < steps; i++)
                {
                    path[i] = NormalizeIndex(previous - (i + 1));
                }
            }
            // if spaces == 0 then path = empty

            Logger.Debug("Move Player", 
                $"Player {mpe.id} moved {mpe.spacesToMove}, from {previous} to {previous+mpe.spacesToMove}, normalized: {next}", 
                LogCategory.Gameplay, this);
            playerPositions[mpe.id] = next;
            playerMovedChannel?.RaiseEvent(new PlayerMovedEvent(mpe.id, previous, next, path));
            // Throws an event if the player has a negative move.
            // This may need a refactor if anything causes the player to move backwards normally.
            //fixed bc only forward movement through a full wrap-around should trigger passedGo
            if (spaces > 0 && next < previous)
            {
                passedGoChannel?.RaiseEvent(mpe.id);
            }
        }

        //Helper methods
        //confirms board is set to default size
        private void EnsureBoard()
        {
            if(boardSpaces == null || boardSpaces.Length == 0)
                InitializeBoard();
        }

        //Normalizes Index for board spaces
        private int NormalizeIndex(int raw)
        {
            int n = boardSpaces?.Length ?? 0;
            if (n == 0) return 0;
            int m = raw % n;
            return (m < 0) ? m + n : m;
        }

        public void OnMoveToSpaceEvent(MoveToSpaceEvent moveToSpaceEvent)
        {
            if (moveToSpaceEvent == null)
            {
                Logger.Warn("BoardManager.OnMoveToSpaceEvent",
                    "moveToSpaceEvent is null",
                    LogCategory.Gameplay,
                    this);
                return;
            }

            Player player = moveToSpaceEvent.player;

            if (player == null)
            {
                Logger.Warn("BoardManager.OnMoveToSpaceEvent",
                    "Player is null",
                    LogCategory.Gameplay,
                    this);
                return;
            }

            switch (moveToSpaceEvent.targetKind)
            {
                case MoveToSpaceCardEffect.TargetSpaceType.CardSpace:
                    MovePlayerToClosestSpaceType(player, typeof(CardSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.ChargeSpace:
                    MovePlayerToClosestSpaceType(player, typeof(ChargeSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.GoForLaunchSpace:
                    MovePlayerToClosestSpaceType(player, typeof(GoForLaunchSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.GoSpace:
                    MovePlayerToClosestSpaceType(player, typeof(GoSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.GravityAssistSpace:
                    MovePlayerToClosestSpaceType(player, typeof(GravityAssistSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.InstrumentSpace:
                    MovePlayerToClosestSpaceType(player, typeof(InstrumentSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.LaunchPadSpace:
                    MovePlayerToClosestSpaceType(player, typeof(LaunchPadSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace:
                    MovePlayerToClosestSpaceType(player, typeof(PlanetSpaceData));
                    break;

                case MoveToSpaceCardEffect.TargetSpaceType.PropertySpace:
                    MovePlayerToClosestSpaceType(player, typeof(PropertySpaceData));
                    break;
                default:
                    Logger.Warn("BoardManager.OnMoveToSpaceEvent",
                    "Unknown target space type.",
                    LogCategory.Gameplay,
                    this);
                    break;
            }
        }

        private void MovePlayerToClosestSpaceType(Player player, Type targetSpaceType)
        {
            int playerId = player.GetId();
            int currentIdx = GetPlayerPosition(playerId);
            int boardLength = boardSpaces.Length;

            int stepsForward = 0;

            for (int i = 1; i <= boardLength; i++)
            {
                int idx = NormalizeIndex(currentIdx + i);
                SpaceData spaceAtIdx = boardSpaces[idx];

                if (spaceAtIdx != null && targetSpaceType.IsInstanceOfType(spaceAtIdx))
                {
                    stepsForward = i;
                    break;
                }
            }

            if (stepsForward <= 0)
            {
                Logger.Warn("BoardManager.MovePlayerToClosestSpaceType",
                    $"No space of type {targetSpaceType.Name} found on the board.",
                    LogCategory.Gameplay,
                    this);
                return;
            }

            MovePlayer(new MovePlayerEvent(playerId, stepsForward));
        }
    }
}
