using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.Rules;
using Events.EventDataStructures;
using Logging;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Logger = Logging.Logger;

public class PlayerManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] public PlayerEventChannel playerAddedEventChannel;
    [SerializeField] public PlayerEventChannel playerRemovedEventChannel;
    [SerializeField] public IntEventChannel initializePlayerCountChannel;
    [SerializeField] public IntEventChannel passedGoChannel;
    [SerializeField] public MoneyDistributionEventChannel payAllPlayersEventChannel;
    [SerializeField] public MoneyDistributionEventChannel collectFromAllPlayersEventChannel;
    [SerializeField] public JailStateChangedEventChannel jailStateChangedEventChannel;
    [SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel;
    [SerializeField] public PayPlayerEventChannel payPlayerEventChannel;
    [SerializeField] public BooleanEventChannel playerDataUpdatedEventChannel;

    public List<Player> players = new List<Player>();
    private StandardRuleSet activeRuleset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // added this to decouple GameManager from PlayerManager to use events instead - hdathert
        initializePlayerCountChannel?.Subscribe(InitializePlayers);
        passedGoChannel?.Subscribe(PassedGo);
        payAllPlayersEventChannel?.Subscribe(OnPayAllPlayersEvent);
        collectFromAllPlayersEventChannel?.Subscribe(OnChargeAllPlayersEvent);
        jailStateChangedEventChannel?.Subscribe(OnJailStateChangedEvent);
        chargePlayerEventChannel?.Subscribe(OnChargePlayerEvent);
        payPlayerEventChannel?.Subscribe(OnPayPlayerEvent);
    }

    void OnDestroy()
    {
        initializePlayerCountChannel?.Unsubscribe(InitializePlayers);
        passedGoChannel?.Unsubscribe(PassedGo);
        payAllPlayersEventChannel?.Unsubscribe(OnPayAllPlayersEvent);
        collectFromAllPlayersEventChannel?.Unsubscribe(OnChargeAllPlayersEvent);
        jailStateChangedEventChannel?.Unsubscribe(OnJailStateChangedEvent);
        chargePlayerEventChannel?.Unsubscribe(OnChargePlayerEvent);
        payPlayerEventChannel.Unsubscribe(OnPayPlayerEvent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Initializes given number of Players. For now, just assigns "Player X" to the
    /// name, where X is the player number. Eventually, should be enhanced to allow
    /// for assigning names and/or colors.
    /// </summary>
    /// <param name="numPlayers">Number of players to initialize</param>
    public void InitializePlayers(int numPlayers)
    {
        Logging.Logger.Info("PlayerManager.InitializePlayers",
            $"Creating players: {numPlayers}",
            LogCategory.Gameplay, 
            this);

        players.Clear();  //prevent duplicates when starting new game
        EnsureDependencies();
        int startingMoney = activeRuleset.PlayerStartingMoney();
        int startingPosition = 0; //GO

        for (int i = 0; i < numPlayers; i++)
        {
            // I think making the Player a scriptable object is the wrong move,
            // since its meant to be a data class, not an Asset in the browser.
            // We might rethink this later, if we create a list of specific
            // "Player" archetypes to get around naming issues, like Monopoly's
            // pieces. In that case, we should probably do something totally
            // different for creating players.
            Player newPlayer = ScriptableObject.CreateInstance<Player>();
            
            newPlayer.SetId(i);
            // doing i+1 so that the name is Player 1, 2, 3, etc.
            newPlayer.SetPName($"Player {i+1}");
            // setting money should be done somewhere else, I think...
            //dzadroga - added basic setting money here, can change after we set up 
            //System to track money, but might be easiest to set cash here to make
            //sure it happens each time, we can definitely move it as we get futher along
            newPlayer.SetMoney(startingMoney);
            newPlayer.SetPosition(startingPosition);
            newPlayer.SetColor(Random.ColorHSV());

            //Defaults added for monoploy
            newPlayer.SetInJail(false);
            newPlayer.SetJailTurns(0);
            newPlayer.SetDoublesInRow(0);

            //The other basic card initialization as well as basic property tracking 
            //could be set up here as we continue to develop game.
            //Just adding these as a placeholder for the starting points the system can 
            //build on as we develop.  
            //examples
            // newPlayer.SetGetOutOfJailFree_Chance(0);
            // newPlayer.SetGetOutOfJailFree_Community(0);
            // newPlayer.ClearOwnedProperties();
            
            players.Add(newPlayer);
            
            //notify event channel listeners of added player 
            if (playerAddedEventChannel != null)
            {
                playerAddedEventChannel.RaiseEvent(newPlayer);
            }

            //Log confirmation
            Logging.Logger.Info("PlayerManager.InitializePlayers",
                $"Initialized {newPlayer.GetPName()} with ${newPlayer.GetMoney()}.",
                LogCategory.Gameplay,
                this);
        }
    }

    /// <summary>
    /// Gets a Player object by ID.
    /// </summary>
    /// <param name="playerId">ID of the player to get.</param>
    /// <returns>Player ScriptableObject, or null if ID not found.</returns>
    public Player GetPlayer(int playerId)
    {
        if (players != null && playerId >= 0 && playerId < players.Count)
            return players[playerId];
        else
        {
            Logging.Logger.Error("PlayerManager.GetPlayer",
                $"Attempted access of playerID out of bounds: {playerId}",
                LogCategory.Gameplay,
                this);
            return null;
        }
    }

    /// <summary>
    /// Returns a List of all Player ScriptableObjects.
    /// </summary>
    /// <returns>List of all Player ScriptableObjects</returns>
    public List<Player> GetAllPlayers()
    {
        var playersCopy = new List<Player>();
        
        foreach (var player in players)
            playersCopy.Add(player);
        
        return playersCopy;
    }

    //us103task122: create removal behavior to allow players to be removed from game start
    public bool RemovePlayer(int playerId)
    {
        //player checker first
        if (playerId < 0 || playerId >= players.Count)
        {
            Logging.Logger.Warn("PlayerManager.RemovePlayer",
                $"RemovePlayer functionality invalid id={playerId}. No action.",
                LogCategory.Gameplay,
                this);
            return false;
        }

        Player removedPlayer = players[playerId];
        players.RemoveAt(playerId);

        //id == list index from GetPlayer functionality
        for (int i = playerId; i < players.Count; i++)
        {
            players[i].SetId(i);
        }

        Logging.Logger.Info("PlayerManager.RemovePlayer",
            $"Removed player with id={playerId}.",
            LogCategory.Gameplay,
            this);

        playerRemovedEventChannel?.RaiseEvent(removedPlayer);
        return true;
    }

    /// <summary>
    /// Event listener for the passed go channel. Kept seperate from add money
    /// TODO: Refactor so magic number is stored elsewhere
    /// </summary>
    /// <param name="id"></param>
    public void PassedGo(int id)
    {
        EnsureDependencies();
        AddMoney(id, activeRuleset.GOSalary()); 
    }

    /// <summary>
    /// Adds money to player object
    /// </summary>
    /// <param name="id"></param>
    /// <param name="money"></param>
    public void AddMoney(int id, int money)
    {       
        GetPlayer(id).AddMoney(money);
    }

    // Just added this for symmetry. Remove if not needed. 
    // Right now, this function is probably not needed, however I am leaving it until we can discuss it. 
    // Updated to properly check FinancialStatus return value for now.
    public void RemoveMoney(int id, int money)
    {
        if (GetPlayer(id).TrySpend(money) == Player.FinancialStatus.Success) { return; }

        // THis is probably not correct. It should instead probably raise an event saying if it succeeds or fails. 
        else
        {
          Logging.Logger.Info("PlayerManager.RemoveMoney",
          $"Player with ID: {id} did not have enough money.",
          LogCategory.Gameplay,
          this);
        }
    }

    public void OnPayAllPlayersEvent(MoneyDistributionEvent payAllPlayersEvent)
    {
        Player player = payAllPlayersEvent.Player;
        int Amount = payAllPlayersEvent.Amount;

        if (player == null) return;

        foreach (Player currentPlayer in players)
        {
            if (player == currentPlayer) RemoveMoney(currentPlayer.GetId(), Amount * (players.Count - 1));
            else AddMoney(currentPlayer.GetId(), Amount);
        }
    }

    public void OnChargeAllPlayersEvent(MoneyDistributionEvent collectFromAllPlayersEvent)
    {
        Player player = collectFromAllPlayersEvent.Player;
        int Amount = collectFromAllPlayersEvent.Amount;

        if (player == null)
        {
            Logging.Logger.Warn("PlayerManager.OnChargeAllPlayers",
                "Player is null",
                LogCategory.Gameplay,
                this);
            return;
        }

        foreach (Player currentPlayer in players)
        {
            if (player == currentPlayer) AddMoney(currentPlayer.GetId(), player.GetMoney() + (Amount * (players.Count - 1)));
            else RemoveMoney(currentPlayer.GetId(), Amount);
        }
    }

    public void OnJailStateChangedEvent(JailStateChangedEvent jailStateChangedEvent)
    {
        Player player = jailStateChangedEvent.player;

        if (player == null)
        {
            Logging.Logger.Warn("PlayerManager.OnChargeAllPlayers",
                "Player is null",
                LogCategory.Gameplay,
                this);
            return;
        }

        player.SetInJail(jailStateChangedEvent.inJail);
        player.SetJailTurns(jailStateChangedEvent.jailTurns);
    }

    public void OnChargePlayerEvent(ChargePlayerEvent chargePlayerEvent)
    {
        Logger.Info("PlayerManager.OnChargePlayerEvent",
            $"Charging {chargePlayerEvent.chargedPlayer.GetPName()} ${chargePlayerEvent.chargeAmount}",
            LogCategory.Economy, this);
        
        RemoveMoney(chargePlayerEvent.chargedPlayer.GetId(), chargePlayerEvent.chargeAmount);
        
        playerDataUpdatedEventChannel.RaiseEvent(true);
    }

    public void OnPayPlayerEvent(PayPlayerEvent payPlayerEvent)
    {
        Logger.Info("PlayerManager.OnPayPlayerEvent",
            $"Paying {payPlayerEvent.paidPlayer.GetPName()} ${payPlayerEvent.amountPaid}",
            LogCategory.Economy, this);
        
        AddMoney(payPlayerEvent.paidPlayer.GetId(), payPlayerEvent.amountPaid);
        
        playerDataUpdatedEventChannel.RaiseEvent(true);
    }

    private void EnsureDependencies()
    {
        if (activeRuleset == null)
            activeRuleset = StandardRuleSet.GetInstance();
    }
}
