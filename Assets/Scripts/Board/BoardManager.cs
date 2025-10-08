using System;
using System.Collections.Generic;
using UnityEngine;
using PsycheOpoly.Events;

namespace PsycheOpoly.Board{

    public class BoardManager : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int defaultBoardSize = 10; 

        //Task 81 create Space[] array
        private Space[] spaces;
        
        //Task 85 player position dictionary
        private readonly Dictionary<int, int> playerPositions = new Dictionary<int, int>();

        //Task 88 subscribe 
        private void OnEnable()  => GameEvents.PlayerMoved += OnPlayerMoved; 

        //Task 89 unsubscribe 
        private void OnDisable()  => GameEvents.PlayerMoved -= OnPlayerMoved;

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
        }

        //Task 84 which is GetSpace(int) with a wrap around
        public Space GetSpace(int index)
        {
            if (spaces == null || spaces.Length == 0)
                throw new InvalidOperationException("Board not initialized, call InitializeBoard()");
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
        public int MovePlayer(int playerID, int spacesToMove)
        {
            EnsureBoard();
            int next = NormalizeIndex(GetPlayerPosition(playerID) + spacesToMove);
            playerPositions[playerID] = next;
            return next;
        }

        //Task 90 event handler
        private void OnPlayerMoved(int playerID, int spacesToMove) => MovePlayer(playerID, spacesToMove);


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
