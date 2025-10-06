using System;
using System.Collections.Generic;
using UnityEngine;
using PsycheOpoly.Events;

namespace PsycheOpoly.Board{

    public class BoardManger : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int defaultBoardSize = 10; 
        
        //Task 88 subscribe 
        private void OnEnable()  => GameEvents.PlayerMoved += OnPlayerMoved; 

        //Task 89 unsubscribe 
        private void OneDisable()  => GameEvents.PlayerMoved -= OnPlayerMoved;

        //Task 90 event handler
        private void OnPlayerMoved(int playerID, int spacesToMove) => MovePlayer(playerID, spacesToMove);
    }

}
