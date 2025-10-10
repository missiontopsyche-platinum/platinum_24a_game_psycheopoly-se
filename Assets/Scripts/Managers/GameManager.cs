using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    //adding in reference to Enum created in the Data folder - nnastase us11-t33
    //default setter
    public GameState State { get; set; } = GameState.None;

    //us11-t41 keep one instance of a gamemanager at a time for security
    public static GameManager Instance { get; private set; }

    //us11-t36 allows for gamestate change action
    public event Action<GameState, GameState> GameStateChanged;

    // This class is the MonoBehavior for the game manager. This is a place holder until more details are implented later.

    // In future, we might want to decouple this reference...
    // Also, made these public so I can inject the fields with what I need for testing purposes.
    [Header("Component References")]
    [SerializeField] public PlayerManager playerManager;
    
    [Header("Event Channels")]
    [SerializeField] public EventChannel<Player> turnStartedChannel;

    private int playerCount = 0;
    private int currentPlayer = 0;

    //us11t41 duplicate prevention with Awake() method
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //basically if we recognize an instance of a game that isn't the one in use, end it
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //keeps game object
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //added Initialize by nnastase for us11-t34
    void Start()
    {
        Initialize();
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

        //edited in for us11
        SetState(GameState.WaitingForTurn);

        turnStartedChannel.RaiseEvent(playerManager.GetPlayer(currentPlayer));
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer + 1) % playerCount;
        Player current = playerManager.GetPlayer(currentPlayer);

        turnStartedChannel.RaiseEvent(current);

    }

    //us11-t34 very basic initializer, just initializing GameState...
    public void Initialize()
    {
        SetState(GameState.Initializing);

        //this is where we should load / create board/players/etc

        //this can be deleted, just displaying options for flow.
        SetState(GameState.WaitingForTurn);
        //mini tester
        Debug.Log("Initialize() successfully called — test passed!");

    }

    //transaction stubs for furutre us11 tskss - nnastase
    public void StartGame() => SetState(GameState.WaitingForTurn);
    public void EndGame() => SetState(GameState.GameOver);


    //change state event helper us11-t36
    private void SetState(GameState newState)
    {
        //state change checker
        if (newState == State)
        {
            return;
        }

        //placeholder
        var old = State;
        State = newState;

        GameStateChanged?.Invoke(old, newState);
        //just for testing
        Debug.Log($"[GameManager] State: {old} > {newState}");
    }

    
}
