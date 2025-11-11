using Events.EventDataStructures;
using System.Collections.Generic;
using UnityEngine;

public class CollectFromAllPlayersCardEffect : CardEffect
{
    [SerializeField] public int Amount;
    [SerializeField] public ChargePlayerEventChannel ChargePlayerEventChannel;
    [SerializeField] public PayPlayerEventChannel PayPlayerEventChannel;
    [SerializeField] public PlayerEventChannel AddPlayerlayerEventChannel;
    [SerializeField] public PlayerEventChannel RemovePlayerlayerEventChannel;

    private List<Player> playerRoster = new List<Player>();

    private void OnEnable()
    {
        AddPlayerlayerEventChannel?.Subscribe(AddPlayerToList);
        RemovePlayerlayerEventChannel?.Subscribe(RemovePlayerFromList);
    }

    private void OnDisable()
    {
        AddPlayerlayerEventChannel?.Unsubscribe(AddPlayerToList);
        RemovePlayerlayerEventChannel?.Subscribe(RemovePlayerFromList);
    }

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;

        foreach (Player currentPlayer in playerRoster)
        {
            if (player == currentPlayer) continue;
            ChargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(currentPlayer, Amount));
            PayPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, Amount));
        }
    }
    private void AddPlayerToList(Player player)
    {
        playerRoster.Add(player);
    }

    private void RemovePlayerFromList(Player player)
    {
        playerRoster.Remove(player);
    }
}

