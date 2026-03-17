using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Assets.Scripts.Managers;
using Assets.Scripts.Managers.Rules;
using Events.EventDataStructures;
using Logging;
using System.Collections.Generic;
using System.Data;
using Data;
using UnityEngine;
using Logger = Logging.Logger;

public class PlayerManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] public PlayerEventChannel playerAddedEventChannel;

    public List<Player> players = new ();

    /// <summary>
    /// Bootstraps Players from data passed from GameManager
    /// </summary>
    /// <param name="playerConfigs">List of PlayerConfig packages containing Player data
    /// and if they're a human player or not</param>
    public void InitializePlayers(List<PlayerConfig> playerConfigs)
    {
        Logger.Info("PlayerManager.InitializePlayers",
            $"Creating {playerConfigs.Count} players.",
            LogCategory.Core);
        
        players.Clear(); // double check that players list is cleared

        foreach (var playerConfig in playerConfigs)
        {
            var player = playerConfig.playerData;
            player.SetMoney(1500); // temporary until we have configurable game settings
            player.SetId(players.Count);
            players.Add(player);
            
            playerAddedEventChannel?.RaiseEvent(player);
            Logger.Info("PlayerManager.InitializePlayers",
                $"Initialized {player.GetPName()} with ${player.GetMoney()}.",
                LogCategory.Core);
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
}
