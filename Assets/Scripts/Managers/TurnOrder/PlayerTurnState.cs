using System.Collections.Generic;   
using UnityEngine;

namespace Assets.Scripts.Managers.TurnOrder
{
    //Encapsulates per-player turn flags used by the strategy
    public interface IPlayerTurnState
    {
        bool IsEliminated(int playerIndex);
        bool IsSkippedThisRound(int playerIndex);
        void ConsumeSkip(int playerIndex);

        bool GetExtraTurn(int playerIndex);
        void SetExtraTurn(int playerIndex, bool value);
    }

    //Default in memory implementation
    //This can be chagned once we know all game states later
    public class PlayerTurnState
    {
        private readonly List<bool> eliminated = new();
        private readonly List<int>  skipCounts = new(); //number of turns to skip
        private readonly List<bool> extraTurn = new();

        public PlayerTurnState(int playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                eliminated.Add(false);
                skipCounts.Add(0);
                extraTurn.Add(false);
            }
        }

        private bool InRange(int idx)
        {
            return idx >= 0 && idx < eliminated.Count;
        }

        //Completes task 273
        //This wil have to be updated in the second semester once we add more logic
        public void Eliminate(int idx)
        {
            if (!InRange(idx)) // in the future we need to alert that we're accessing out of bounds players
                return;
            eliminated[idx] = true;
        }
        
        public bool IsEliminated(int playerIndex)           
        { 
            if (!InRange(playerIndex))
                return false; // update to log error for out of bounds
            return eliminated[playerIndex]; 
        }
  
        public void GrantExtraTurn(int idx)
        { 
            if (!InRange(idx))
                return;
            extraTurn[idx] = true;
        }
        
        public bool HasExtraTurn(int playerIndex)           
        { 
            if (!InRange(playerIndex))
                return false; // update to log error for out of bounds
            return extraTurn[playerIndex]; 
        }

        public void SetExtraTurn(int playerIndex, bool val) 
        { 
            if (!InRange(playerIndex))
                return; 
            extraTurn[playerIndex] = val; 
        }
        
        public void AddSkip(int idx, int n)
        {
            if (!InRange(idx))
                return;
            skipCounts[idx] += Mathf.Max(1, n);
        }
        
        public bool IsSkippedThisRound(int playerIndex)     
        { 
            if (!InRange(playerIndex))
                return false; // update to log error for out of bounds
            return skipCounts[playerIndex] > 0; 
        }

        public void ConsumeSkip(int playerIndex)            
        { 
            if (!InRange(playerIndex))
                return;
            if (skipCounts[playerIndex] > 0) 
            {
                skipCounts[playerIndex]--;
            }  
        }
    }
}
