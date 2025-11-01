using System.Collections.Generic;
using UnityEngine;
using Logging;

public class PlayerManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] public PlayerEventChannel playerAddedEventChannel;
    [SerializeField] public PlayerEventChannel playerRemovedEventChannel;
    [SerializeField] public IntEventChannel initializePlayerCountChannel;
    [SerializeField] public IntEventChannel passedGoChannel;

    private List<Player> players = new List<Player>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // added this to decouple GameManager from PlayerManager to use events instead - hdathert
        initializePlayerCountChannel?.Subscribe(InitializePlayers);
        passedGoChannel?.Subscribe(PassedGo);
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
        int startingMoney = 1500; //Amount based on normal Monopoly game
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
        AddMoney(id, 200); 
    }

    /// <summary>
    /// Adds money to player object
    /// </summary>
    /// <param name="id"></param>
    /// <param name="money"></param>
    public void AddMoney(int id, int money)
    {
        int pMoney = GetPlayer(id).GetMoney();
        GetPlayer(id).SetMoney(pMoney + money);
    }

}
