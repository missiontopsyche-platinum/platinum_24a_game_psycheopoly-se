using System;
using System.Collections.Generic;
using UnityEngine;
using Logging;
using Logger = Logging.Logger;

namespace PsycheOpoly.Board
{
    public class BoardManager : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int defaultBoardSize = 40;

        [Header("Render Components")] 
        [SerializeField] public BoardRenderer boardRenderer;

        [Header("Event Channels")]
        [SerializeField] public PlayerEventChannel      playerAddedChannel;
        [SerializeField] public PlayerMovedEventChannel playerMovedChannel;
        [SerializeField] public MovePlayerEventChannel  movePlayerChannel;
        [SerializeField] public IntEventChannel         passedGoChannel;

        [Header("Board Spaces")] [SerializeField]
        public SpaceData[] boardSpaces;
        
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

        private void OnEnable()  => EnsureSubscribed();

        private void Start()
        {
            InitializeBoard();
            movePlayerChannel?.Subscribe(MovePlayer);
        }

        private void OnDisable() => EnsureUnsubscribed();

        private void OnDestroy() => EnsureUnsubscribed();

      
        private void EnsureSubscribed()
        {
            if (subscribed) return;
            if (!this) return;
            playerAddedChannel?.Subscribe(AddPlayer);
            movePlayerChannel?.Subscribe(MovePlayer);
            subscribed = true;
        }

        private void EnsureUnsubscribed()
        {
            if (!subscribed) return;
            playerAddedChannel?.Unsubscribe(AddPlayer);
            movePlayerChannel?.Unsubscribe(MovePlayer);
            subscribed = false;
        }

        // adds a new player. Something weird was happening with the dict and was causing players
        // to "start" at arbitrary indices rippling down to incorrect positioning for the renderer.
        private void AddPlayer(Player player)
        {
            playerPositions.Add(player.GetId(), 0);
            Logger.Debug("AddPlayer", 
                $"Player {player.GetId()} added at position {playerPositions[player.GetId()]}", 
                LogCategory.Gameplay, this);
        }

        //Task 82 create InitializeBoard method 
        public void InitializeBoard(int size = -1)
        {
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
            int next = NormalizeIndex(previous + mpe.spacesToMove);
            Logger.Debug("Move Player", 
                $"Player {mpe.id} moved {mpe.spacesToMove}, from {previous} to {previous+mpe.spacesToMove}, normalized: {next}", 
                LogCategory.Gameplay, this);
            playerPositions[mpe.id] = next;
            playerMovedChannel?.RaiseEvent(new PlayerMovedEvent(mpe.id, previous, next));
            // Throws an event if the player has a negative move.
            // This may need a refactor if anything causes the player to move backwards normally.
            if (next < previous)
            {
                passedGoChannel?.RaiseEvent(mpe.id);
            }
        }

        //Helper methods
        //confirms board is set to default size
        private void EnsureBoard()
        {
            if(boardSpaces == null || boardSpaces.Length == 0)
                InitializeBoard(defaultBoardSize);
        }

        //Normalizes Index for board spaces
        private int NormalizeIndex(int raw)
        {
            int n = boardSpaces?.Length ?? 0;
            if (n == 0) return 0;
            int m = raw % n;
            return (m < 0) ? m + n : m;
        }
    }

}
