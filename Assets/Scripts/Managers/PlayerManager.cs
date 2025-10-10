using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<Player> players = new List<Player>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

            //Log confirmation
            Debug.Log($"Initialized {newPlayer.GetPName()} with ${newPlayer.GetMoney()}.");
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
            Debug.LogError("PlayerManager: GetPlayer " +
                           "attempted access of playerID out" +
                           $"of bounds: {playerId}");
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
}
