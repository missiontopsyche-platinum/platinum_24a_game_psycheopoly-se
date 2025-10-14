//nnastase on 10/9/2025
//covering US-11 task 33, initializing GameState ENUM 
//welcome to changes, just set some standard ideas for likely game states we'll encounter

using UnityEngine;

public enum GameState
{
    None = 0,       //no game state
    Initializing = 1,
    WaitingForTurn = 2,
    PlayerTurn = 3,
    BotTurn = 4,
    GameOver = 5
}
