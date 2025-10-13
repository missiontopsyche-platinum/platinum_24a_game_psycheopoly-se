using UnityEngine;
using System;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    //adding in reference to Enum created in the Data folder - nnastase us11-t33
    //default setter
    public GameState gameState { get; set; } = GameState.None;

    //us11-t41 keep one instance of a gamemanager at a time for security
    public static GameManager instance { get; private set; }

    // In future, we might want to decouple this reference...
    // Also, made these public so I can inject the fields with what I need for testing purposes.
    [Header("Component References")]
    [SerializeField] public PlayerManager playerManager;
    
    [FormerlySerializedAs("gameStateChangeChannel")]
    [Header("Event Channels")]
    //us11-t36 allows for gamestate change action
    [SerializeField] public EventChannel<GameStateChangedEvent> gameStateChangedChannel;
    [SerializeField] public EventChannel<Player> turnStartedChannel;

    private int playerCount = 0;
    private int currentPlayer = 0;

    //us11t41 duplicate prevention with Awake() method
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            //basically if we recognize an instance of a game that isn't the one in use, end it
            Destroy(gameObject);
            return;
        }

        instance = this;
        //keeps game object
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //added Initialize by nnastase for us11-t34
    void Start()
    {
    }

    //start & end game to satisfy us11-35
    //pasing player count for now.
    public void StartGame(int playerCount)
    {
        if (gameState != GameState.None && gameState != GameState.GameOver)
        {
            Debug.LogWarning($"[GameManager] is unable to start game from state: {gameState}");
            return;
        }

        this.playerCount = playerCount;
        Initialize();
        SetUpGame(this.playerCount);
    }

    public void EndGame()
    {
        SetState(GameState.GameOver);
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
        //mini tester
        Debug.Log("Initialize() successfully called — test passed!");

    }

    //transaction stubs for furutre us11 tskss - nnastase
    //public void StartGame() => SetState(GameState.WaitingForTurn);
    //public void EndGame() => SetState(GameState.GameOver);

    //us11t35
    //here we are looking to have a method that allows for valid state transitions for enums
    private bool CanTransition(GameState from, GameState to)
    {
        //our game start transition options
        if (from == GameState.None && to == GameState.Initializing) return true;

        if (from == GameState.Initializing && to == GameState.WaitingForTurn) return true;

        //turn cycling transitin options
        if (from == GameState.WaitingForTurn && to == GameState.PlayerTurn) return true;
        if (from == GameState.PlayerTurn && to == GameState.BotTurn) return true;
        if (from == GameState.BotTurn && to == GameState.WaitingForTurn) return true;

        //end of game and restart transitions options
        //further code will dictate if these transitions are met (for game oveer)
        if (from == GameState.PlayerTurn && to == GameState.GameOver) return true;
        if (from == GameState.BotTurn && to == GameState.GameOver) return true;
        if (from == GameState.WaitingForTurn && to == GameState.GameOver) return true;

        if (from == GameState.GameOver && to == GameState.Initializing) return true;

        return false;
    }


    //change state event helper us11-t36
    private void SetState(GameState newState)
    {
        //state change checker
        if (newState == gameState)
        {
            return;
        }

        //us11-t35 introduce state transition options
        if (!CanTransition(gameState, newState))
        {
            Debug.LogWarning($"[Game Manager] transition not allowed from : {gameState} to {newState}");

            return;
        }

        GameStateChangedEvent gameStateChange = new GameStateChangedEvent(gameState, newState);
        gameState = newState;

        gameStateChangedChannel.RaiseEvent(gameStateChange);
        //just for testing
        Debug.Log($"[GameManager] State: {gameStateChange.newGameState} > {gameStateChange.previousGameState}");
    }

    
}
