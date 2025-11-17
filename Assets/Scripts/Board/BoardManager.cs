using System;
using System.Collections.Generic;
using UnityEngine;
using Logging;
using Logger = Logging.Logger;

namespace PsycheOpoly.Board{

    [ExecuteAlways]
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
            if (size <= 0) size = Mathf.Max(3, defaultBoardSize);
            boardSpaces = new SpaceData[size];
            
            // this is placeholder to keep tests working while we make assets.
            T CreateSpace<T>(Action<T> init) where T : SpaceData
            {
                var space = ScriptableObject.CreateInstance<T>();
                init(space);
                return space;
            }

            boardSpaces[0] = CreateSpace<GoSpaceData>(s =>
            {
                s.spaceName = "GO!";
                s.spaceColor = Color.chartreuse;
            });
            
            for (int i = 1; i < size; i++)
                boardSpaces[i] = (i % 3 == 0) 
                    ? CreateSpace<CardSpaceData>(s =>
                    {
                        s.spaceName = $"Card: Space #{i}";
                        s.spaceColor = Color.lavender;
                    })
                    : CreateSpace<PropertySpaceData>(s =>
                    {
                        s.spaceName = $"Property: Space #{i}";
                        s.spaceColor = Color.peru;
                    });
            
            boardRenderer?.GenerateBoard(boardSpaces);
            
            EnsureSubscribed();
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
