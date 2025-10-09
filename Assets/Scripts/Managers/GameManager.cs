using UnityEngine;

public class GameManager : MonoBehaviour
{
    // This class is the MonoBehavior for the game manager. This is a place holder until more details are implented later.

    // In future, we might want to decouple this reference...
    // Also, made these public so I can inject the fields with what I need for testing purposes.
    [Header("Component References")]
    [SerializeField] public PlayerManager playerManager;
    
    [Header("Event Channels")]
    [SerializeField] public EventChannel<Player> turnStartedChannel;

    private int playerCount = 0;
    private int currentPlayer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Sets up a new game by initializing the PlayerManager and starting the first turn.
    /// </summary>
    /// <param name="playerCount">Number of players for the game.</param>
    public void SetUpGame(int playerCount)
    {
        if (playerCount < 2 || playerCount > 4)
        {
            Debug.LogError("Invalid player count, must be between 2 and 4.");
            return;
        }
        this.playerCount = playerCount;
        currentPlayer = 0;
        playerManager.InitializePlayers(this.playerCount);
        turnStartedChannel.RaiseEvent(playerManager.GetPlayer(currentPlayer));
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer + 1) % playerCount;
        Player current = playerManager.GetPlayer(currentPlayer);
        
        turnStartedChannel.RaiseEvent(current);
    }
}
