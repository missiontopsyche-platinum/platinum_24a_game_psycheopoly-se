using UnityEngine;

namespace Assets.Scripts.Managers.TurnOrder
{
    //Manager that controls the turn index and asks strategy for who goes next
    //Self-wires in EditMode and Play mode testing
    public class TurnCycleManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int playerCount = 4;
        [SerializeField] private int startingPlayerIndex = 0;

        private StandardTurnOrderStrategy strategy = new ();
        private PlayerTurnState playerTurnState;

        public int CurrentPlayerIndex { get; private set; }

        private void Awake()
        {
            ResetCycle(playerCount, startingPlayerIndex);
            CurrentPlayerIndex = startingPlayerIndex;
        }

        public void ResetCycle(int count, int startIndex = 0)
        {
            playerCount = Mathf.Max(2, count);
            CurrentPlayerIndex = Mathf.Clamp(startIndex, 0, playerCount - 1);
            playerTurnState = new PlayerTurnState(playerCount);
        }

        //Call when a turn ends
        //Moves to next player or repeats if extra turn 
        public int Advance()
        {
            int next = strategy.NextPlayerIndex(CurrentPlayerIndex, playerCount, playerTurnState);

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
    }
}
