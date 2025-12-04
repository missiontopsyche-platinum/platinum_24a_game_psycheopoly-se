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

        [Header("Dependencies")]
        [SerializeField] private PlayerTurnState playerTurnState;
        [SerializeField] private GameManager gameManager; //To raise existing events if needed

        private ITurnOrderStrategy strategy = new StandardTurnOrderStrategy();

        public int CurrentPlayerIndex { get; private set; }

        private void Awake()
        {
            EnsureDeps();
            ResetCycle(playerCount, startingPlayerIndex);
            CurrentPlayerIndex = startingPlayerIndex;

        }

        private void EnsureDeps()
        {
            if (!playerTurnState)
                playerTurnState = GetComponent<PlayerTurnState>() ?? gameObject.AddComponent<PlayerTurnState>();

            if (!gameManager)
            {
                gameManager = Object.FindFirstObjectByType<GameManager>();
            }
        }

        public void ResetCycle(int count, int startIndex = 0)
        {
            EnsureDeps();
            playerCount = Mathf.Max(2, count);
            playerTurnState.EnsureSize(playerCount);
            CurrentPlayerIndex = Mathf.Clamp(startIndex, 0, playerCount - 1);
        }

        //Call when a turn ends
        //Moves to next player or repeats if extra turn 
        public int Advance()
        {
            EnsureDeps();
            int next = strategy.NextPlayerIndex(CurrentPlayerIndex, playerCount, playerTurnState);

            CurrentPlayerIndex = next;

            return CurrentPlayerIndex;
        }

        //hooks to alter state from other systems
        public void GrantExtraTurn(int playerIndex) => playerTurnState.GrantExtraTurn(playerIndex);
        public void AddSkip(int playerIndex, int count = 1) => playerTurnState.AddSkip(playerIndex, count);
        public void Eliminate(int playerIndex) => playerTurnState.Eliminate(playerIndex);
    }
}
