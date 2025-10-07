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
        
        //Task 88 subscribe 
        private void OnEnable()  => GameEvents.PlayerMoved += OnPlayerMoved; 

        //Task 89 unsubscribe 
        private void OneDisable()  => GameEvents.PlayerMoved -= OnPlayerMoved;

        //Task 82 create InitializeBoard method 
        public void InitializeBoard(int size = -1)
        {
            if (size <= 0) size = Mathf.Max(3, defaultBoardSize);
            spaces = new Space[size];
        }

        //Task 90 event handler
        private void OnPlayerMoved(int playerID, int spacesToMove) => MovePlayer(playerID, spacesToMove);
    }

}
