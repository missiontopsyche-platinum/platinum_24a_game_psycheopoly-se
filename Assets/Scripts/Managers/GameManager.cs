using Assets.Scripts.Managers.TurnFlow;
using Assets.Scripts.Managers.TurnOrder;
using Data;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //adding in reference to Enum created in the Data folder - nnastase us11-t33
    //default setter
    public GameState gameState { get; private set; } = GameState.None;

    //us11-t41 keep one instance of a gamemanager at a time for security
    public static GameManager instance { get; private set; }

    [Header("Manager References")]
    [SerializeField] private PlayerManager playerManager;

    private TurnCycleManager turnCycleManager;
    private TurnFlowCoordinator turnFlowCoordinator;
    private int playerCount = 0;

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

    private void Start()
    {
        if (gameState == GameState.None)
            StartGame();
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

        Logging.Logger.Info("GameManager.TryChangeState",
            $"[GameManager] State changed: {old} -> {newState}",
            LogCategory.Gameplay,
            this);
        return true;
    }

    //start & end game to satisfy us11-35
    //pasing player count for now.
    public void StartGame()
    {
        if (gameState != GameState.None && gameState != GameState.GameOver)
        {
            Logging.Logger.Warn("GameManager.StartGame",
                $"Cannot start game from state: {gameState}",
                LogCategory.Gameplay,
                this);
            return;
        }

        if (playerManager == null)
        {
            Logging.Logger.Warn("GameManager.StartGame",
                "PlayerManager reference is missing.",
                LogCategory.Core,
                this);
            return;
        }

        Initialize();
        SetUpGame();
    }

    public void EndGame()
    {
        StopAllCoroutines();

        if (!TryChangeState(GameState.GameOver))
            return;

        Logging.Logger.Info("GameManager.EndGame",
            "Game ended.",
            LogCategory.Core,
            this);
    }

    public void Initialize()
    {
        if (!TryChangeState(GameState.Initializing))
            return;

        Logging.Logger.Debug("GameManager.Initialize",
            "Game initialization started.",
            LogCategory.Core,
            this);
    }



    /// <summary>
    /// Sets up a new game by initializing the PlayerManager and starting the first turn.
    /// </summary>
    public void SetUpGame()
    {
        playerCount = 4; // temporary until real setup exists

        InitializePlayers_Temporary();

        turnCycleManager = new TurnCycleManager(playerCount);
        InitializeTurnFlowCoordinator();

        if (!TryChangeState(GameState.WaitingForTurn))
            return;

        StartCoroutine(WaitForGameInit());
    }

    /// <summary>
    /// This is a demonstration/stop-gap method to handle player creation while we don't have
    /// a proper game setup configuration solution, which involves manually creating players
    /// for testing purposes.
    ///
    /// In the future, we'll either extract or construct the PlayerConfig from the configuration
    /// and call InitializePlayers in PlayerManager in exactly the same way. The call is blocking,
    /// so it can operate in the sequence needed by GameManager when setting up the game.
    /// </summary>
    private void InitializePlayers_Temporary()
    {
        Player MakePlayer(string name, Color color)
        {
            var player = ScriptableObject.CreateInstance<Player>();
            player.SetPName(name);
            player.SetColor(color);
            return player;
        }
        
        var configs = new List<PlayerConfig>
        {
            new (MakePlayer("Player 1", Color.red), true, null),
            new (MakePlayer("Player 2", Color.blue), false, 
                ScriptableObject.CreateInstance<AIBehaviorWeights>()),
            new (MakePlayer("Player 3", Color.yellow), false, 
                ScriptableObject.CreateInstance<AIBehaviorWeights>()),
            new (MakePlayer("Player 4", Color.green), false, 
                ScriptableObject.CreateInstance<AIBehaviorWeights>())
        };
        
        playerManager.InitializePlayers(configs);
    }

    /// <summary>
    /// This is exposed to allow for testing in editmode, bypasses the init wait.
    /// Completes any asynchronous game initialization and starts the first player's turn.
    /// </summary>
    public void CompleteGameInit()
    {
        BeginTurnSystem();
    }

    private IEnumerator WaitForGameInit()
    {
        yield return new WaitForSeconds(2f);
        CompleteGameInit();
    }


    private void InitializeTurnFlowCoordinator()
    {
        turnFlowCoordinator = GetComponent<TurnFlowCoordinator>();

        if (turnFlowCoordinator == null)
            turnFlowCoordinator = gameObject.AddComponent<TurnFlowCoordinator>();

        // turnFlowCoordinator.Initialize(turnCycleManager); TODO: uncomment when merging with US868

        Logging.Logger.Info("GameManager.InitializeTurnFlowCoordinator",
            "TurnFlowCoordinator initialized by GameManager.",
            LogCategory.Core,
            this);
    }

    private void BeginTurnSystem()
    {
        if (turnCycleManager == null)
        {
            Logging.Logger.Error("GameManager.BeginTurnSystem",
                "TurnCycleManager was not initialized.",
                LogCategory.Core,
                this);
            return;
        }

        if (turnFlowCoordinator == null)
        {
            Logging.Logger.Error("GameManager.BeginTurnSystem",
                "TurnFlowCoordinator was not initialized.",
                LogCategory.Core,
                this);
            return;
        }


    private bool IsPlayerTurn(Player player)
    {
        if (player == null) return false;

        int requestId = player.GetId();
        // Validate current turn order
        return matchesTurnCycle;
    }
}
