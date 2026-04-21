using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers.TurnOrder
{
    // Manager that controls the turn index, utilized by GameManager and TurnFlowCoordinator
    public class TurnCycleManager
    {
        public int CurrentPlayerIndex { get; private set; }
        
        private readonly int playerCount;
        private readonly PlayerTurnState playerTurnState;
        public int CurrentTurnNumber { get; private set; }

        public TurnCycleManager(int playerCount)
        {
            this.playerCount = Mathf.Max(2, playerCount);
            CurrentPlayerIndex = 0;
            playerTurnState = new PlayerTurnState(playerCount);
            CurrentTurnNumber = 1;
            GameManager.instance.CurrentRoundNumber = 1;
        }

        //Call when a turn ends
        //Moves to next player or repeats if extra turn 
        public int Advance()
        {
            int previousPlayerIndex = CurrentPlayerIndex;
            int next = NextPlayerIndex(CurrentPlayerIndex);

            CurrentPlayerIndex = next;
            CurrentTurnNumber++;

            if (next <= previousPlayerIndex)
            {
                GameManager.instance.CurrentRoundNumber++;
            }

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
        
        private bool HasExtraTurn(int playerIndex)
        {
            return playerTurnState.HasExtraTurn(playerIndex);
        }

        public void PruneBankruptPlayers(IReadOnlyList<Player> players)
        {
            if (players == null) return;

            int count = Mathf.Min(playerCount, players.Count);

            for (int i = 0; i < count; i++)
            {
                Player player = players[i];

                if (player == null)
                    continue;

                if (player.IsMarkedBankrupt() && !playerTurnState.IsEliminated(i))
                {
                    playerTurnState.Eliminate(i);
                }
            }
        }

        public bool IsPlayerEliminated(int playerIndex)
        {
            return playerTurnState.IsEliminated(playerIndex);
        }

        public int GetActivePlayerCount()
        {
            int activeCount = 0;

            for (int i = 0; i < playerCount; i++)
            {
                if (!playerTurnState.IsEliminated(i))
                {
                    activeCount++;
                }
            }

            return activeCount;
        }

        public int GetLastRemainingPlayerIndex()
        {
            int remainingPlayer = -1;
            int activeCount = 0;

            for (int i = 0; i < playerCount; i++)
            {
                if (playerTurnState.IsEliminated(i))
                    continue;

                remainingPlayer = i;
                activeCount++;

                if (activeCount > 1)
                    return -1;
            }

            return activeCount == 1 ? remainingPlayer : -1;
        }
    }
}
