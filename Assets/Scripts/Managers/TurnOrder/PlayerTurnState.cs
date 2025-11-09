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
    public class PlayerTurnState : MonoBehaviour, IPlayerTurnState
    {
        [SerializeField] private List<bool> eliminated = new();
        [SerializeField] private List<int>  skipCounts = new(); //number of turns to skip
        [SerializeField] private List<bool> extraTurn  = new();

        public void EnsureSize(int count)
        {
            while (eliminated.Count < count) eliminated.Add(false);
            while (skipCounts.Count < count) skipCounts.Add(0);
            while (extraTurn.Count  < count) extraTurn.Add(false);
        }


        //IPlayerTurnState
        public bool IsEliminated(int playerIndex)           
        { 
            EnsureSize(playerIndex + 1); 
            return eliminated[playerIndex]; 
        }

        public bool IsSkippedThisRound(int playerIndex)     
        { 
            EnsureSize(playerIndex + 1); 
            return skipCounts[playerIndex] > 0; 
        }

        public void ConsumeSkip(int playerIndex)            
        { 
            EnsureSize(playerIndex + 1); 
            if (skipCounts[playerIndex] > 0) 
            {
                skipCounts[playerIndex]--;
            }  
        }

        public bool GetExtraTurn(int playerIndex)           
        { 
            EnsureSize(playerIndex + 1); 
            return extraTurn[playerIndex]; 
        }

        public void SetExtraTurn(int playerIndex, bool val) 
        { 
            EnsureSize(playerIndex + 1); 
            extraTurn[playerIndex] = val; 
        }
    }
}
