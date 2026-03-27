using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Logging;
using PsycheOpoly.Board;
using Assets.Scripts.Managers.Movement;
using Assets.Scripts.Managers.TurnOrder;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.Purchase;
using Data;
using Assets.Scripts.Managers.TurnFlow;
using Events.EventDataStructures;

public class GameManager : MonoBehaviour
{
    //adding in reference to Enum created in the Data folder - nnastase us11-t33
    //default setter
    public GameState gameState { get; set; } = GameState.None;

    /// <summary>
    /// Current phase within the active player's turn.
    /// This is driven by <see cref="TryChangeTurnPhase"/> to ensure
    /// that all transitions follow the allowed turn-phase finite state machine.
    /// </summary>
    public TurnPhase turnPhase { get; set; } = TurnPhase.None;

    //us11-t41 keep one instance of a gamemanager at a time for security
    public static GameManager instance { get; private set; }

    [Header("UI Elements")] [SerializeField]
    public DiceRollPanelController diceRollPanel;
    
    [Header("Event Channels")]
    //us11-t36 allows for gamestate change action
    [SerializeField] public GameStateChangedEventChannel gameStateChangedChannel;
    [SerializeField] public TurnStartedEventChannel turnStartedChannel;
    [SerializeField] public DiceRolledEventChannel diceRolledChannel;
    [SerializeField] public BooleanEventChannel pieceMoveCompletedChannel;
    [SerializeField] public CardDrawnEventChannel cardDrawnChannel;
    [SerializeField] public BooleanEventChannel turnEndedChannel;
    [SerializeField] public BooleanEventChannel spaceResolutionCompletedChannel;
    [SerializeField] public IntEventChannel bankruptPlayerEventChannel;

    [Header("Space Resolution Event Channels")]
    [SerializeField] public ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;
    [SerializeField] public BooleanEventChannel playerDataUpdatedEventChannel;

    [Header("Manager References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private StandardMovementStrategy movementStrategy;
    [SerializeField] private PlayerManager playerManager;
    
    public TurnCycleManager turnCycleManager; // this shouldn't be public, but needs to be for now to get it to turn flow coordinator

    private int playerCount = 0;

    //The below are for testing that the event is properly registering in the class
    public int dieOne = 0;
    public int dieTwo = 0;
    public int totalRolled = 0;

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

    /// <summary>
    /// Defines the legal transitions between <see cref="TurnPhase"/> values for
    /// the turn finite state machine.
    ///
    /// Each key is the current phase, and its value is the set of phases that are
    /// allowed as the next phase. All transitions must go through
    /// <see cref="TryChangeTurnPhase"/> which validates against this map, and
    /// also requires the overall <see cref="GameState"/> to be
    /// <see cref="GameState.PlayerTurn"/>.
    /// </summary>
    private static readonly Dictionary<TurnPhase, HashSet<TurnPhase>> PhaseAllowed = new()
    {
        { TurnPhase.None,            new() { TurnPhase.StartTurn } },
        { TurnPhase.StartTurn,       new() { TurnPhase.PreRoll } },
        { TurnPhase.PreRoll,         new() { TurnPhase.RollingDice } },
        { TurnPhase.RollingDice,     new() { TurnPhase.MovingPiece } },
        { TurnPhase.MovingPiece,     new() { TurnPhase.ResolvingSpace, TurnPhase.MovingPiece, TurnPhase.ResolvingCards } },
        { TurnPhase.ResolvingSpace,  new() { TurnPhase.ResolvingCards, TurnPhase.MovingPiece, TurnPhase.PostTurn } },
        { TurnPhase.ResolvingCards,  new() { TurnPhase.PostTurn, TurnPhase.MovingPiece } },
        { TurnPhase.PostTurn,        new() { TurnPhase.EndTurn } },
        { TurnPhase.EndTurn,         new() { } },
        //TurnFlowCoordinator now manually fires a new TurnStarted
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
        cardDrawnChannel?.Subscribe(OnCardDrawnEvent);
        turnEndedChannel?.Subscribe(OnTurnEndedEvent);
        spaceResolutionCompletedChannel?.Subscribe(OnSpaceResolutionCompleted);
        bankruptPlayerEventChannel?.Subscribe(OnBankruptPlayer);
        // US 555 TODO: Scaffold comment for PropertyPurchaseRequestEventChannel
        // US 555 TODO: Scaffold comment for PropertyPurchaseAcceptedEventChannel
        // US 555 TODO: Scaffold comment for PropertyPurchaseRejectedEventChannel

        // hook up to temporary methods for handling rent/purchase
        chargeOwnershipFeeEventChannel?.Subscribe(QuickRent);


        //us253-t278 hook up movement strategy to dice/board managers
        if (movementStrategy != null)
        {
            movementStrategy.enabled = true;

            Logging.Logger.Info("GameManager.Start", "StandardMovementStrategy active and listening.",
                LogCategory.Core, this);
        }
        else
        {
            Logging.Logger.Warn("GameManager.Start", "StandardMovementStrategy not assigned in Inspector.",
                LogCategory.Core, this);
        }

        if (boardManager == null)
            Logging.Logger.Warn("GameManager.Start", "BoardManager reference not assigned.",
                LogCategory.Core, this);
    }

    //start & end game to satisfy us11-35
    //pasing player count for now.
    public void StartGame()
    {
        if (gameState != GameState.None && gameState != GameState.GameOver)
        {
            Logging.Logger.Warn("GameManager.StartGame",
                $"[GameManager] is unable to start game from state: {gameState}",
                LogCategory.Gameplay,
                this);
            return;
        }

        this.playerCount = 4; // hard coding for now, until we get proper game setup configuration
        Initialize();
        SetUpGame();
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
            StartGame();
    }

    /// <summary>
    /// Called when the object is destroyed. 
    /// </summary>
    public void OnDestroy()
    {
        //Unsubscribe from event channels
        diceRolledChannel?.Unsubscribe(DiceRolled);
        pieceMoveCompletedChannel?.Unsubscribe(PieceMoveCompleted);
        cardDrawnChannel?.Unsubscribe(OnCardDrawnEvent);
        turnEndedChannel?.Unsubscribe(OnTurnEndedEvent);
        spaceResolutionCompletedChannel?.Unsubscribe(OnSpaceResolutionCompleted);
        // US 555 TODO: Scaffold comment for PropertyPurchaseRequestEventChannel
        // US 555 TODO: Scaffold comment for PropertyPurchaseAcceptedEventChannel
        // US 555 TODO: Scaffold comment for PropertyPurchaseRejectedEventChannel

        // unhook temporary methods for handling rent/purchase
        chargeOwnershipFeeEventChannel?.Unsubscribe(QuickRent);
    }

    /// <summary>
    /// Listener for the <see cref="pieceMoveCompletedChannel"/> that advances the
    /// turn FSM from <see cref="TurnPhase.MovingPiece"/> to
    /// <see cref="TurnPhase.ResolvingSpace"/> once the piece movement animation
    /// has finished.
    ///
    /// If <paramref name="pieceMoveCompleted"/> is false, the FSM does not
    /// advance.
    /// </summary>
    public void PieceMoveCompleted(bool pieceMoveCompleted)
    {
        // in future, should have a state machine for turn progress
        if (!pieceMoveCompleted || turnPhase != TurnPhase.MovingPiece) return;

        Logging.Logger.Debug("GameManager.PieceMoveCompleted",
            "Piece movement completed, advancing turn phase.",
            LogCategory.Gameplay,
            this);
        TryChangeTurnPhase(TurnPhase.ResolvingSpace);
        movementStrategy?.OnPieceMoveCompleted(pieceMoveCompleted);
    }


    /// <summary>
    /// Sets up a new game by initializing the PlayerManager and starting the first turn.
    /// </summary>
    public void SetUpGame()
    {
        InitializePlayers_Temporary(); // this directly creates the data needed for PlayerManager to create players now, we don't need the event channel.

        turnCycleManager = new TurnCycleManager(this.playerCount);
        

        //edited in for us11
        SetState(GameState.WaitingForTurn);
        
        // Wait for game to init
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
        StartTurn();
    }

    private IEnumerator WaitForGameInit()
    {
        yield return new WaitForSeconds(2f);
        CompleteGameInit();
    }

    /// <summary>
    /// Begins the current player's turn, entering the turn-phase FSM.
    ///
    /// This method:
    /// - Sets <see cref="gameState"/> to <see cref="GameState.PlayerTurn"/>.
    /// - Transitions the FSM from <see cref="TurnPhase.None"/> (or
    ///   <see cref="TurnPhase.NextTurn"/>) to <see cref="TurnPhase.StartTurn"/>.
    /// - Raises <see cref="turnStartedChannel"/> for HUD and other systems.
    /// - Enables the dice roll panel UI.
    /// - Advances to <see cref="TurnPhase.PreRoll"/>, where the game
    ///   waits for a roll dice request.
    /// </summary>
    private void StartTurn()
    {
        // temporary, assume every player is a 'human' player
        SetState(GameState.PlayerTurn);
        if (TryChangeTurnPhase(TurnPhase.StartTurn))
        {
            Logging.Logger.Debug("GameManager.StartTurn",
                    "None finished, entering StartTurn.",
                    LogCategory.Gameplay, this);
            int active = turnCycleManager?.CurrentPlayerIndex ?? 0;
            turnStartedChannel.RaiseEvent(new TurnStartedEvent(active, 0)); // turnNum not tracked here
            //diceRollPanel?.gameObject.SetActive(true);
            // This is the "waiting" for dice roll phase, replacing the busy wait.
            Logging.Logger.Debug("GameManager.StartTurn",
                    "StartTurn finished, entering PreRoll.",
                    LogCategory.Gameplay, this);
            TryChangeTurnPhase(TurnPhase.PreRoll);
        }
    }

    /// <summary>
    /// Advances to the next player's turn in the turn order, cycling the turn FSM.
    ///
    /// This method attempts to move the FSM from <see cref="TurnPhase.PostTurn"/>
    /// to <see cref="TurnPhase.EndTurn"/> (via <see cref="OnTurnEndedEvent"/>),
    /// then from <see cref="TurnPhase.EndTurn"/> to
    /// <see cref="TurnPhase.NextTurn"/>. Once in <see cref="TurnPhase.NextTurn"/>,
    /// it increments <see cref="currentPlayer"/> and <see cref="currentTurn"/>,
    /// and calls <see cref="StartTurn"/> to re-enter the pre-roll phase for the
    /// next player.
    /// </summary>
    /// TURN FLOW COORDINATOR WILL FIRE NEXT TURN AUTOMATICALLY
    //public void NextTurn()
    //{
    //    if (TryChangeTurnPhase(TurnPhase.NextTurn))
    //    {
    //        Logging.Logger.Debug("GameManager.NextTurn",
    //            "EndTurn finished, entering NextTurn.",
    //            LogCategory.Gameplay, this);

    //        currentPlayer = (currentPlayer + 1) % playerCount;
    //        currentTurn++;
    //        StartTurn();
    //    }
    //}


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
    /// 
    /// Attempts to transition from <see cref="TurnPhase.RollingDice"/> to
    /// <see cref="TurnPhase.MovingPiece"/> using
    /// <see cref="TryChangeTurnPhase"/>.
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

        dieOne = diceRolledEvent.dieOne;
        dieTwo = diceRolledEvent.dieTwo;
        totalRolled = diceRolledEvent.totalRoll;


        if (turnPhase != TurnPhase.RollingDice)
        {
            Logging.Logger.Warn("GameManager.DiceRolled",
                $"Ignoring dice roll in phase {turnPhase}",
                LogCategory.Gameplay, this);
            return;
        }

        if (this.gameState == GameState.PlayerTurn)
        {
            // movement now handled by StandardMovementStrategy. no need to raise MovePlayerEvent manually.
            Logging.Logger.Debug("gameManager.DiceRolled", "Dice roll processed movement handled by " +
                "StandardMovementStrategy.",
                Logging.LogCategory.Gameplay, this);
            TryChangeTurnPhase(TurnPhase.MovingPiece);
            movementStrategy?.OnDiceRolled(diceRolledEvent);
        }


        //added for US395 in prep for task398; TurnFlowCoordinator will use TurnStrategy for a doubles roll
        if (dieOne == dieTwo && turnCycleManager != null)
        {
            turnCycleManager.GrantExtraTurn(turnCycleManager.CurrentPlayerIndex);
        }

    }

    /// <summary>
    /// Listener for <see cref="rollDiceRequestedChannel"/> raised by the UI
    /// (e.g., the dice roll button).
    ///
    /// When <paramref name="diceRollRequestedEvent"/> is true, this attempts to
    /// transition the FSM from <see cref="TurnPhase.PreRoll"/> to
    /// <see cref="TurnPhase.RollingDice"/>. The dice animation and actual roll are
    /// then handled by the dice/animation systems.
    /// </summary>
    /// <param name="diceRollRequestedEvent">
    /// True when a roll has been requested; false is ignored.
    /// </param>
    public void OnRollDiceRequest(bool diceRollRequestedEvent)
    {
        if (!diceRollRequestedEvent || turnPhase != TurnPhase.PreRoll) return;

        if (TryChangeTurnPhase(TurnPhase.RollingDice))
            //diceManager?.RollDice();
        Logging.Logger.Debug("GameManager.OnRollDiceRequest",
            "Dice roll requested, entering RollingDice.",
            LogCategory.Gameplay, this);
    }

    public void OnPropertyPurchaseRequest() // TODO: add PropertyPurchaseRequestEvent(event) in param
    {
        // This scaffold method is a placeholder for handling property purchase requests.
        // Full implementation will validate the request, checking game state,
        // player turn, and turn phase, before delegating to PurchaseManager.
        // Also not sure where the event should be raised, PurchaseManager or GameManager.
        /*if (Event.player == null || Event.Title == null)
        {
            PropertyPurchaseRejectedEventChannel?.RaiseEvent("Error: Missing purchase context.");
            return;
        }

        if (!IsPlayerTurn(Event.Player))
        {
            PropertyPurchaseRejectedEventChannel.RaiseEvent(Message="Error: Invalid player turn.");
            return;
        }

        if (turnPhase != TurnPhase.ResolvingSpace)
        {
            PropertyPurchaseRejectedEventChannel.RaiseEvent(Message = "Error: Illegal TurnPhase");
            return;
        }

        if (!purchaseManager.TryHandlePurchase(Event.Player, Event.Tile))
        {
            PropertyPurchaseRejectedEventChannel.RaiseEvent(Message = "Error: Purchase execution failed.");
            return;
        }

        if (TryChangeTurnPhase(TurnPhase.PostTurn))
        {
            Logging.Logger.Debug("GameManager.OnPropertyPurchaseRequest",
                "Property purchase request during ResolvingSpace, entering PostTurn.",
                LogCategory.Gameplay, 
                this);

            PropertyPurchaseAcceptedEventChannel.RaiseEvent(Message = "Success: Property purchase accepted.");
        }*/
    }

    /// <summary>
    /// Extremely quick and temporary implementation of rent logic for the end-of-semester
    /// prototype, to be fully replaced by the strategy pattern for handling rules.
    /// </summary>
    /// <param name="cofe"></param>
    public void QuickRent(ChargeOwnershipFeeEvent cofe)
    {
        if (turnPhase != TurnPhase.ResolvingSpace)
            return;

        int playerMoney = cofe.fromPlayer.GetMoney();
        int actualPayment = Mathf.Min(cofe.amount, playerMoney);  // pay what you can for now...
    
        cofe.fromPlayer.SetMoney(playerMoney - actualPayment);
        cofe.toPlayer.SetMoney(cofe.toPlayer.GetMoney() + actualPayment);
    
        if (actualPayment < cofe.amount)
        {
            // player cant pay full rent.
            // we'll handle bankruptcy etc in Semester 2
            Logging.Logger.Warn("GameManager.QuickRent",
                $"Player {cofe.fromPlayer.GetPName()} could only pay ${actualPayment} of ${cofe.amount} rent " +
                $"to {cofe.toPlayer.GetPName()} on {cofe.sourceSpace.spaceName}",
                LogCategory.Economy, this);
        }
        else
        {
            Logging.Logger.Info("GameManager.QuickRent",
                $"{cofe.fromPlayer.GetPName()} paid ${actualPayment} rent to {cofe.toPlayer.GetPName()} " +
                $"for {cofe.sourceSpace.spaceName}",
                LogCategory.Economy, this);
        }
        
        playerDataUpdatedEventChannel.RaiseEvent(true);
        TryChangeTurnPhase(TurnPhase.PostTurn);
    }

    /// <summary>
    /// Listener for <see cref="cardDrawnChannel"/> fired when a player draws a
    /// card from a deck.
    ///
    /// This advances the turn FSM from <see cref="TurnPhase.ResolvingSpace"/> to
    /// <see cref="TurnPhase.ResolvingCards"/>, indicating that card effects are
    /// now being processed. Card resolution logic is handled elsewhere; this
    /// method only updates the phase.
    /// </summary>
    /// <param name="card">The card that was drawn.</param>
    /// <param name="player">The player who drew the card.</param>
    /// <param name="deck">The deck the card was drawn from.</param>
    public void OnCardDrawnEvent(Card card, Player player, CardDeck deck)
    {
        if (card == null || player == null || deck == null) return;

        TryChangeTurnPhase(TurnPhase.ResolvingCards);
        Logging.Logger.Debug("GameManager.OnCardDrawnEvent",
            "Card drawn, entering ResolvingCards.",
            LogCategory.Gameplay, this);
    }

    /// <summary>
    /// Listener for <see cref="turnEndedChannel"/> raised by the End Turn button
    /// in the UI (e.g., <c>TurnPanelController</c>).
    ///
    /// When <paramref name="turnEndedEvent"/> is true, this attempts to move the
    /// FSM from <see cref="TurnPhase.PostTurn"/> to
    /// <see cref="TurnPhase.EndTurn"/>. If that transition is allowed, it then
    /// calls <see cref="NextTurn"/> to advance to the next player's
    /// <see cref="TurnPhase.StartTurn"/>/PreRoll cycle.
    ///
    /// If the current phase is not allowed to transition to
    /// <see cref="TurnPhase.EndTurn"/>, or if the flag is false, no change is
    /// made.
    /// </summary>
    /// <param name="turnEndedEvent">
    /// True when the End Turn action has been confirmed; false is ignored.
    /// </param>
    public void OnTurnEndedEvent(bool turnEndedEvent)
    {
        if (!turnEndedEvent) return;

        if (turnPhase == TurnPhase.PostTurn && TryChangeTurnPhase(TurnPhase.EndTurn))
        {
            Logging.Logger.Debug("GameManager.OnTurnEndedEvent",
                "Turn ended, entering EndTurn.",
                LogCategory.Gameplay, this);
            //turnCycleManager?.Advance();
            int nextPlayer = (turnCycleManager != null) ? turnCycleManager.Advance() : 0;
            turnStartedChannel?.RaiseEvent(new TurnStartedEvent(nextPlayer, 0));
            //NextTurn(); // TODO: kept this until we can fully delegate turn cycling to TurnCycleManager
        }
    }

    /// <summary>
    /// Handles completion of all space-resolution logic during the player's turn.
    ///
    /// This method is invoked when the <c>spaceResolutionCompletedChannel</c>
    /// signals that a space's effects (purchase, rent, cards, taxes, etc.)
    /// have fully resolved. 
    ///
    /// If the current <see cref="turnPhase"/> is <see cref="TurnPhase.ResolvingSpace"/>
    /// or <see cref="TurnPhase.ResolvingCards"/>, the method attempts to
    /// transition the FSM into <see cref="TurnPhase.PostTurn"/>. This marks the
    /// end of all mid-turn resolution and enables the player to end their turn
    /// or perform any UI actions associated with PostTurn.
    /// </summary>
    /// <param name="completed">
    /// A boolean flag raised by the space resolution event channel.
    /// Expected to be <c>true</c> when resolution has concluded.
    /// </param>
    private void OnSpaceResolutionCompleted(bool completed)
    {
        if (!completed) return;

        if (turnPhase == TurnPhase.ResolvingSpace || turnPhase == TurnPhase.ResolvingCards)
        {
            if (TryChangeTurnPhase(TurnPhase.PostTurn))
            {
                Logging.Logger.Debug("GameManager.OnSpaceResolutionCompleted",
                    "Resolution finished, entering PostTurn.",
                    LogCategory.Gameplay, this);
            }
        }
    }

    /// <summary>
    /// Attempts to transition the per-turn FSM to a new <see cref="TurnPhase"/>.
    /// 
    /// If both checks pass and <paramref name="newPhase"/> differs from the
    /// current <see cref="turnPhase"/>, the phase is updated and the method
    /// returns true.
    /// </summary>
    /// <param name="newPhase">The next turn phase.</param>
    /// <returns>
    /// True if the phase was changed; false if the transition was illegal or
    /// redundant.
    /// </returns>
    private bool TryChangeTurnPhase(TurnPhase newPhase)
    {
        if (newPhase == turnPhase) return false;

        if (gameState != GameState.PlayerTurn)
        {
            Logging.Logger.Error("GameManager.TryChangeTurnPhase",
                $"Illegal action, game state must be {GameState.PlayerTurn} to proceed. Current state: {gameState}",
                LogCategory.Gameplay,
                this);
            return false;
        }

        if (!PhaseAllowed.TryGetValue(turnPhase, out var nexts) ||
            !nexts.Contains(newPhase))
        {
            Logging.Logger.Warn("GameManager.TryChangeTurnPhase",
                $"Illegal turn phase transition: {turnPhase} -> {newPhase}",
                LogCategory.Gameplay,
                this);
            return false;
        }

        var prev = turnPhase;
        turnPhase = newPhase;

        return true;
    }

    //Handle the bankruptPlayerEventChannel input. Need to verify that the playerID is also it's turn order. 
    private void OnBankruptPlayer(int player)
    {
        turnCycleManager.Eliminate(player);
    }

    private bool IsPlayerTurn(Player player)
    {
        if (player == null) return false;

        int requestId = player.GetId();
        // Validate current turn order
        bool matchesTurnCycle = requestId == turnCycleManager?.CurrentPlayerIndex;
        return matchesTurnCycle;
    }
}
