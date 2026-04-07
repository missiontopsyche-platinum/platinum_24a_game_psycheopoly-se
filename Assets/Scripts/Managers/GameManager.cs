using Assets.Scripts.Managers.TurnFlow;
using Assets.Scripts.Managers.TurnOrder;
using Data;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //us11-t41 keep one instance of a gamemanager at a time for security
    public static GameManager instance { get; private set; }

    [Header("Manager References")]
    [SerializeField] private PlayerManager playerManager;
    private TurnFlowCoordinator turnFlowCoordinator;
    private TurnCycleManager turnCycleManager;

    [Header("Scene Flow")]
    [SerializeField] private int winSceneIndex = 2;

    private int playerCount = 0;

    public static int LastWinningPlayerId { get; private set; } = -1;
    public static string LastWinningPlayerName { get; private set; } = string.Empty;

    // In Project Settings/Script Execution Order, this has been moved below all other
    // scripts to ensure they have the time to set themselves up on init before we
    // try to start a game. This solves race conditions we introduced when we
    // decoupled the GameManager from all other systems.
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
        StartGame();
    }

    //start & end game to satisfy us11-35
    //pasing player count for now.
    public void StartGame()
    {
        if (playerManager == null)
        {
            Logging.Logger.Warn("GameManager.StartGame",
                "PlayerManager reference is missing.",
                LogCategory.Core,
                this);
            return;
        }

        SetUpGame();
    }

    public void EndGame()
    {
        StopAllCoroutines();

        Logging.Logger.Info("GameManager.EndGame",
            "Game ended.",
            LogCategory.Core,
            this);

        if (SceneTransitioner.instance != null)
        {
            SceneTransitioner.instance.TransitionScene(winSceneIndex);
        }
        else
        {
            // fallback if no scene transitioner exists in scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(winSceneIndex);
        }
    }



    /// <summary>
    /// Sets up a new game by initializing the PlayerManager and starting the first turn.
    /// </summary>
    public void SetUpGame()
    {
        LastWinningPlayerId = -1;
        LastWinningPlayerName = string.Empty;

        // if there are no game configs loaded, do the temp one.
        // this allows us to start the game from the game screen quickly for testing.
        if (GameConfiguration.playerConfigs == null) 
            InitializePlayers_Temporary();
        else 
            playerManager.InitializePlayers(GameConfiguration.playerConfigs);

        playerCount = playerManager.GetPlayerCount();
        
        turnCycleManager = new TurnCycleManager(playerCount);
        InitializeTurnFlowCoordinator();

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

        turnFlowCoordinator.Initialize(turnCycleManager, playerManager.GetAllPlayers());

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

        Logging.Logger.Info("GameManager.BeginTurnSystem",
            "Initialization complete. Handing control to TurnFlowCoordinator.",
            LogCategory.Core,
            this);

        turnFlowCoordinator.StartGame();
    }

    public void SetWinner(Player winner)
    {
        LastWinningPlayerId = winner != null ? winner.GetId() : -1;
        LastWinningPlayerName = winner != null ? winner.GetPName() : "No Winner";

        Logging.Logger.Info("GameManager.SetWinner",
            $"Winner set to: {LastWinningPlayerName}",
            LogCategory.Core,
            this);
    }
}
