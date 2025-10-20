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
        [SerializeField] private BoardRenderer boardRenderer;

        [Header("Event Channels")]
        [SerializeField] public PlayerMovedEventChannel playerMovedChannel;
        [SerializeField] public MovePlayerEventChannel  movePlayerChannel;
        [SerializeField] public IntEventChannel         passedGoChannel;
        [SerializeField] private EventChannel<DiceRolledEvent> diceRolledEvent;

        [Header("Render Components")]
        [SerializeField] private BoardRenderer boardRenderer;

        //Task 81 create Space[] array
        private Space[] spaces;
        
        //For Testing purposes
        public int BoardSize => spaces?.Length ?? 0;

        //Task 85 player position dictionary
        private readonly Dictionary<int, int> playerPositions = new Dictionary<int, int>();

        //Task 88 subscribe and Task 89 unsubscribe 
        private bool _subscribed;

        private void Awake()
        {
            EnsureSubscribed();
            
            Logger.Initialize(LogSettings.Current());
        }

        private void OnEnable()  => EnsureSubscribed();

        private void Start()
        {
            InitializeBoard();
        }

        private void OnDisable() => EnsureUnsubscribed();

        private void OnDestroy() => EnsureUnsubscribed();

        private void Start()
        {
            movePlayerChannel.Subscribe(MovePlayer);
        }

        private void EnsureSubscribed()
        {
            if (_subscribed) return;
            if (!this) return;
            //GameEvents.PlayerMoved += OnPlayerMoved;
            //movePlayerChannel.Subscribe(MovePlayer);
            _subscribed = true;
        }

        private void EnsureUnsubscribed()
        {
            if (!_subscribed) return;
            movePlayerChannel.Unsubscribe(MovePlayer);
            _subscribed = false;
        }

        //Task 82 create InitializeBoard method 
        public void InitializeBoard(int size = -1)
        {
            if (size <= 0) size = Mathf.Max(3, defaultBoardSize);
            spaces = new Space[size];
            
            //Task 83 which will fill the board with mix of placeholder spaces
            //This will be changed after final rule confirmation from stakeholder
            spaces[0] = new GoSpace("Go");
            for (int i = 1; i < size; i++)
                spaces[i] = (i % 3 == 0) ? new ChanceSpace("Chance")
                                         : new PropertySpace($"Property {i}");
            
            boardRenderer?.GenerateBoard(spaces);
            
            EnsureSubscribed();
        }

        //Task 84 which is GetSpace(int) with a wrap around
        public Space GetSpace(int index)
        {
            if (spaces == null || spaces.Length == 0)
            {
                Logging.Logger.Error("BoardManager.GetSpace",
                    "Board not initialized, call InitializeBoard()",
                    LogCategory.Gameplay,
                    this);
                throw new InvalidOperationException("Board not initialized, call InitializeBoard()");
            }
            return spaces[NormalizeIndex(index)];
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
            if (spaceIndex < 0 || spaceIndex >= spaces.Length)
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
            return playerPositions.TryGetValue(playerID, out var idx) ? idx : 0;
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
            if(spaces == null || spaces.Length == 0)
                InitializeBoard(defaultBoardSize);
        }

        //Normalizes Index for board spaces
        private int NormalizeIndex(int raw)
        {
            int n = spaces?.Length ?? 0;
            if (n == 0) return 0;
            int m = raw % n;
            return (m < 0) ? m + n : m;
        }
    }

}
