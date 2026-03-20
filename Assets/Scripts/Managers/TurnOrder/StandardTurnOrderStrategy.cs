namespace Assets.Scripts.Managers.TurnOrder
{
    //Standard Monopoly turn order
    //If current player has an extra turn then they go again and flag is cleared
    //else it goes to next non eliminated player consuming one skip if present
    //Wraps around the player list.

    public class StandardTurnOrderStrategy
    {
        public bool HasExtraTurn(int currentIndex, PlayerTurnState state)
        {
            return state.HasExtraTurn(currentIndex);
        }

        public int NextPlayerIndex(int currentIndex, int playerCount, PlayerTurnState state)
        {
            //If the current player has an extra turn then clear it and keep same index
            if (state.HasExtraTurn(currentIndex))
            {
                state.SetExtraTurn(currentIndex, false);
                return currentIndex;
            }

            //Otherwise move forward until there is a playable player
            int attempts = 0;
            int idx = currentIndex;

            while (attempts < playerCount)
            {
                idx = (idx + 1) % playerCount;
                attempts++;

                if (state.IsEliminated(idx)) continue;

                if (state.IsSkippedThisRound(idx))
                {
                    state.ConsumeSkip(idx);
                    continue;
                }

                return idx; //found next
            }

            //If everyone is eliminated or skipped then just return current
            return currentIndex;
        }
    }
}
