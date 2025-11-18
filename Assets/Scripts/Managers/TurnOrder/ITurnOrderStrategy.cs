namespace Assets.Scripts.Managers.TurnOrder
{
    public interface ITurnOrderStrategy
    {
        //Returns the next player index given current states
        int NextPlayerIndex(int currentIndex, int playerCount, IPlayerTurnState state);

        //Checks if player should immediately take another turn
        bool HasExtraTurn(int currentIndex, IPlayerTurnState state); 
    }
}