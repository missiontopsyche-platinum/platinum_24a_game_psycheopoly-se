using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Logging;

public class GameManager : MonoBehaviour
{
    //adding in reference to Enum created in the Data folder - nnastase us11-t33
    //default setter
    public GameState gameState { get; set; } = GameState.None;

    //us11-t41 keep one instance of a gamemanager at a time for security
    public static GameManager instance { get; private set; }

    [Header("UI Elements")] [SerializeField]
    public DiceRollPanelController diceRollPanel;
    
    [Header("Event Channels")]
    //us11-t36 allows for gamestate change action
    [SerializeField] public GameStateChangedEventChannel gameStateChangedChannel;
    [SerializeField] public TurnStartedEventChannel turnStartedChannel;
    [SerializeField] public PlayerMovedEventChannel playerMovedChannel;
    [SerializeField] public IntEventChannel initializePlayerCountChannel;
    [SerializeField] public DiceRolledEventChannel diceRolledChannel;
    [SerializeField] public MovePlayerEventChannel movePlayerChannel;
    [SerializeField] public BooleanEventChannel pieceMoveCompletedChannel;
    

    private int playerCount = 0;
    private int currentPlayer = 0;
    private int currentTurn = 0;

    //The below are for testing that the event is properly registering in the class
    public int dieOne = 0;
    public int dieTwo = 0;
    public int totalRolled = 0;

    private Coroutine currentTurnCoroutine;
    private bool turnComplete = false;

    // Task 111 legal state transition map
    private static readonly Dictionary<GameState, HashSet<GameState>> Allowed = new()
    {
        { GameState.None,            new HashSet<GameState>{ GameState.Initializing } },
        { GameState.Initializing,    new HashSet<GameState>{ GameState.WaitingForTurn } },
        { GameState.WaitingForTurn,  new HashSet<GameState>{ GameState.PlayerTurn, GameState.GameOver } },
        { GameState.PlayerTurn,      new HashSet<GameState>{ GameState.PlayerTurn, GameState.BotTurn, GameState.GameOver } },
        { GameState.BotTurn,         new HashSet<GameState>{ GameState.WaitingForTurn, GameState.GameOver } },
        { GameState.GameOver,        new HashSet<GameState>{ GameState.Initializing } },
    };

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

    //Task 112 which is a guarded transition API
    public bool TryChangeState(GameState newState)
    {
        if (newState == gameState) return false;

        if (!Allowed.TryGetValue(gameState, out var nexts) || !nexts.Contains(newState))
        {
            Logging.Logger.Warn("GameManager.TryChangeState",
                $"[GameManager] Illegal transition: {gameState} -> {newState}",
                LogCategory.Gameplay,
                this);
            return false;
        }

        var old = gameState;
        gameState = newState;

        if (gameStateChangedChannel != null)
            gameStateChangedChannel.RaiseEvent(new GameStateChangedEvent(old, newState));

        Logging.Logger.Info("GameManager.TryChangeState",
            $"[GameManager] State changed: {old} -> {newState}",
            LogCategory.Gameplay,
            this);
        return true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //added Initialize by nnastase for us11-t34
    void Start()
    {
        //US156T157 subscribe to DiceRolled Listener
        diceRolledChannel.Subscribe(DiceRolled);
        pieceMoveCompletedChannel?.Subscribe(PieceMoveCompleted);
    }

    //start & end game to satisfy us11-35
    //pasing player count for now.
    public void StartGame(int playerCount)
    {
        if (gameState != GameState.None && gameState != GameState.GameOver)
        {
            Logging.Logger.Warn("GameManager.StartGame",
                $"[GameManager] is unable to start game from state: {gameState}",
                LogCategory.Gameplay,
                this);
            return;
        }

        this.playerCount = playerCount;
        Initialize();
        SetUpGame(this.playerCount);
    }

    public void EndGame()
    {
        StopAllCoroutines();
        SetState(GameState.GameOver);
    }


    // Update is called once per frame
    void Update()
    {
        // us232 t238 start the game once everything is loaded in- happens once.
        // this is *not* a good solution long term and is simply to demonstrate
        // current prototype progress.
        if (gameState == GameState.None)
            StartGame(4);
    }

    /// <summary>
    /// Called when the object is destroyed. 
    /// </summary>
    public void OnDestroy()
    {
       //Unsubscribe from event channels
       diceRolledChannel.Unsubscribe(DiceRolled);
    }

    private void PieceMoveCompleted(bool pieceMoveCompleted)
    {
        // in future, should have a state machine for turn progress
        turnComplete = pieceMoveCompleted;
    }

    /// <summary>
    /// Sets up a new game by initializing the PlayerManager and starting the first turn.
    /// </summary>
    /// <param name="playerCount">Number of players for the game.</param>
    public void SetUpGame(int playerCount)
    {
        if (playerCount < 2 || playerCount > 4)
        {
            Logging.Logger.Error("GameManager.SetUpGame",
                "Invalid player count, must be between 2 and 4.",
                LogCategory.Gameplay,
                this);
            gameState = GameState.None;
            return;
        }
        this.playerCount = playerCount;
        currentPlayer = 0;
        initializePlayerCountChannel.RaiseEvent(playerCount); // raises event for player count

        //edited in for us11
        SetState(GameState.WaitingForTurn);
        
        // Wait for game to init
        StartCoroutine(WaitForGameInit());
    }

    private IEnumerator WaitForGameInit()
    {
        yield return new WaitForSeconds(2f);
        StartTurn();
    }

    private void StartTurn()
    {
        // temporary, assume every player is a 'human' player
        SetState(GameState.PlayerTurn);
        turnStartedChannel.RaiseEvent(new TurnStartedEvent(currentPlayer, currentTurn));
        turnComplete = false;
        currentTurnCoroutine = StartCoroutine(ExecuteTurn());
    }

    public void NextTurn()
    {
        if (currentTurnCoroutine != null)
            StopCoroutine(currentTurnCoroutine);
        
        currentPlayer = (currentPlayer + 1) % playerCount;
        currentTurn++;
        StartTurn();
    }

    private IEnumerator ExecuteTurn()
    {
        diceRollPanel.gameObject.SetActive(true);

        // we should move through the state machine over time, this is a way to wait per frame
        // to check for event fires.
        while (!turnComplete)
            yield return new WaitForEndOfFrame(); // busy wait for turn to complete (event fire etc)
        
        // turn is complete, call next turn
        NextTurn();
    }

    //us11-t34 very basic initializer, just initializing GameState...
    public void Initialize()
    {
        SetState(GameState.Initializing);

        // TODO Why is this even here? hdathert
        //this is where we should load / create board/players/etc
        //mini tester
        Logging.Logger.Debug("GameManager.Initialize",
            "Initialize() successfully called - test passed!",
            LogCategory.Core,
            this);

    }

    //transaction stubs for furutre us11 tskss - nnastase
    //public void StartGame() => SetState(GameState.WaitingForTurn);
    //public void EndGame() => SetState(GameState.GameOver);

    
    // TODO this seems like it might be redundant with the HashMap implementaion at the top of this file.
    // We might want to refactor this to use that, as it seems like it would be a much more maintainable
    // solution long-term.
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
        if (from == GameState.PlayerTurn && to == GameState.PlayerTurn) return true;
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
        if (newState == gameState) return;
        if (!CanTransition(gameState, newState))
        {
            Logging.Logger.Warn("GameManager.SetState",
                $"Illegal transition: {gameState} -> {newState}",
                LogCategory.Gameplay,
                this);
            return;
        }

        var prev = gameState;
        gameState = newState;

        //null safe raise so tests for testing
        gameStateChangedChannel?.RaiseEvent(new GameStateChangedEvent(prev, newState));

        //Corrected Order
        Logging.Logger.Debug("GameManager.SetState",
            $"State: {prev} > {newState}",
            LogCategory.Gameplay,
            this);
    }

    /// <summary>
    /// Dice Rolled event listener. Takes the DiceRolledEvent pushed by the event channel and
    /// then will utilize the contents as necessary. 
    /// For now, it just logs the details and saves the amounts to a class variable 
    /// </summary>
    /// <param name="diceRolledEvent"></param>
    /// <returns>DiceRolledEvent object</returns>
    public void DiceRolled(DiceRolledEvent diceRolledEvent)
    {
        // Refactor to use US145 Logger
        Logging.Logger.Info("gameManager.DiceRolled",
            "Die One: " + diceRolledEvent.dieOne,
            Logging.LogCategory.Gameplay);
        Logging.Logger.Info("gameManager.DiceRolled", 
            "Die Two: " + diceRolledEvent.dieTwo,
            Logging.LogCategory.Gameplay);
        Logging.Logger.Info("gameManager.DiceRolled",
            "Total: " + diceRolledEvent.totalRoll,
            Logging.LogCategory.Gameplay);

        this.dieOne = diceRolledEvent.dieOne;
        this.dieTwo = diceRolledEvent.dieTwo;
        this.totalRolled = diceRolledEvent.totalRoll;

        if (this.gameState == GameState.PlayerTurn)
        {
            movePlayerChannel?.RaiseEvent(new MovePlayerEvent(this.currentPlayer, this.totalRolled));
        }
    }
}
