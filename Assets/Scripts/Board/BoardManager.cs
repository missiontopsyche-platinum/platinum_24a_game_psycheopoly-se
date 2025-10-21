using System;
using System.Collections.Generic;
using UnityEngine;
using Logging;

namespace PsycheOpoly.Board{

    [ExecuteAlways]
    public class BoardManager : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int defaultBoardSize = 10;

        [Header("Event Channels")]
        [SerializeField] public PlayerMovedEventChannel playerMovedChannel;
        [SerializeField] public MovePlayerEventChannel movePlayerChannel;

        //Task 81 create Space[] array
        private Space[] spaces;
        
        //For Testing purposes
        public int BoardSize => spaces?.Length ?? 0;

        //Task 85 player position dictionary
        private readonly Dictionary<int, int> playerPositions = new Dictionary<int, int>();

        //Task 88 subscribe and Task 89 unsubscribe 
        private bool _subscribed;

        private void Awake()     => EnsureSubscribed();
        private void OnEnable()  => EnsureSubscribed();
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
            movePlayerChannel?.Subscribe(MovePlayer);
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

        //Task 96 SetPlayerPosition(int, int) method
        public void SetPlayerPosition(int playerID, int spaceIndex)
        {
            EnsureBoard();
            playerPositions[playerID] = NormalizeIndex(spaceIndex);
        }

        //Task 86 GetPlayerPosition(int) method
        public int GetPlayerPosition(int playerID)
        {
            EnsureBoard();
            return playerPositions.TryGetValue(playerID, out var idx) ? idx : 0;
        }

        //Task 87 MovePlayer and Module wrap
        public void MovePlayer(MovePlayerEvent mpe)
        {
            EnsureBoard();
            int previous = GetPlayerPosition(mpe.id);
            int next = NormalizeIndex(previous + mpe.spacesToMove);
            playerPositions[mpe.id] = next;
            playerMovedChannel?.RaiseEvent(new PlayerMovedEvent(mpe.id, previous, next));
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
