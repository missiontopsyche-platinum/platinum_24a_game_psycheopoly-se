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
            
            players.Add(newPlayer);
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
}
