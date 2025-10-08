using System;
using System.Collections.Generic;
using UnityEngine;
using PsycheOpoly.Events;

namespace PsycheOpoly.Board{

    public class BoardManger : MonoBehaviour
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
        private void OneDisable()  => GameEvents.PlayerMoved -= OnPlayerMoved;

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
        public space GetSpace(int index)
        {
            if (spaces == null || spaces.Length == 0)
                throw new InvalidOperationException("Board not initialized, call InitializeBoard()");
            return spaces[NormalizeIndex(index)];
        }

        //Task 90 event handler
        private void OnPlayerMoved(int playerID, int spacesToMove) => MovePlayer(playerID, spacesToMove);
    }

}
