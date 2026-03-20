using UnityEngine;

namespace Assets.Scripts.Managers.TurnOrder
{
    // Manager that controls the turn index, utilized by GameManager and TurnFlowCoordinator
    public class TurnCycleManager
    {
        public int CurrentPlayerIndex { get; private set; }
        
        private readonly int playerCount;
        private PlayerTurnState playerTurnState;

        public TurnCycleManager(int playerCount)
        {
            playerCount = Mathf.Max(2, playerCount);
            CurrentPlayerIndex = 0;
            playerTurnState = new PlayerTurnState(playerCount);
        }

        //Call when a turn ends
        //Moves to next player or repeats if extra turn 
        public int Advance()
        {
            int next = NextPlayerIndex(CurrentPlayerIndex);

            CurrentPlayerIndex = next;

            return CurrentPlayerIndex;
        }

        //hooks to alter state from other systems
        public void GrantExtraTurn(int playerIndex) => playerTurnState.GrantExtraTurn(playerIndex);
        public void AddSkip(int playerIndex, int count = 1) => playerTurnState.AddSkip(playerIndex, count);
        public void Eliminate(int playerIndex) => playerTurnState.Eliminate(playerIndex);

        // For AI use
        public void SyncCurrentPlayerIndex(int playerIndex)
        {
            CurrentPlayerIndex = Mathf.Clamp(playerIndex, 0, playerCount - 1);
        }

        private int NextPlayerIndex(int currentIndex)
        {
            //If the current player has an extra turn then clear it and keep same index
            if (HasExtraTurn(currentIndex))
            {
                playerTurnState.SetExtraTurn(currentIndex, false);
                return currentIndex;
            }

            //Otherwise move forward until there is a playable player
            int attempts = 0;
            int idx = currentIndex;

            while (attempts < playerCount)
            {
                idx = (idx + 1) % playerCount;
                attempts++;

                if (playerTurnState.IsEliminated(idx)) continue;

                if (playerTurnState.IsSkippedThisRound(idx))
                {
                    playerTurnState.ConsumeSkip(idx);
                    continue;
                }

                return idx; //found next
            }

            //If everyone is eliminated or skipped then just return current
            return currentIndex;
        }
        
        private bool HasExtraTurn(int currentIndex)
        {
            return playerTurnState.HasExtraTurn(currentIndex);
        }
    }
}
