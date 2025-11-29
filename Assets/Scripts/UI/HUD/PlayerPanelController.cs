using UnityEngine;
using UnityEngine.UI;
using Logging;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class PlayerPanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] public TurnStartedEventChannel turnStartedChannel;
    // TODO: Refactor if new PlayerEventChannel gets added
    [SerializeField] public PlayerEventChannel addPlayerEventChannel;

    public List<Player> playersList;

    [Header("UI Elements")]
    [SerializeField] public Text playerNameText;
    [SerializeField] public Text playerMoneyText;

    private void OnEnable()
    {
        Subscribe(turnStartedChannel, DisplayCurrentPlayer);
        Subscribe(addPlayerEventChannel, AddPlayer);
        Logging.Logger.Trace("PlayerPanelController.OnEnable",
            "Player panel is now enabled.",
            LogCategory.UI,
            this);
    }

    private void OnDisable()
    {
        ClearSubscriptions();
        Logging.Logger.Trace("PlayerPanelController.OnDisable",
            "Player panel is now disabled.",
            LogCategory.UI,
            this);
    }

    public void DisplayCurrentPlayer(TurnStartedEvent turnStartedEvent)
    {
        if (turnStartedEvent == null)
        {
            Logging.Logger.Warn("PlayerPanelController.DisplayCurrentPlayer",
                $"Event: {nameof(turnStartedEvent)} is null",
                LogCategory.UI,
                this);
            return;
        }

        int currentPlayerId = turnStartedEvent.playerId;
        var player = playersList?.Find(player => player.GetId() == currentPlayerId);

        if (player == null)
        {
            SetTextSafe(playerNameText, "Player: -");
            SetTextSafe(playerMoneyText, "Money: $-");
            Logging.Logger.Warn("PlayerPanelController.DisplayCurrentPlayer",
                $"Player ID: {currentPlayerId}, not found",
                LogCategory.UI,
                this);
            return;
        }

        var name = player.GetPName();
        var money = player.GetMoney();

        SetTextSafe(playerNameText, $"Player: {name}");
        SetTextSafe(playerMoneyText, $"Money: {money}");
        Logging.Logger.Info("PlayerPanelController.DisplayCurrentPlayer",
            $"Current Player: {name} with ${money}",
            LogCategory.UI,
            this);
    }

    public void AddPlayer(Player player)
    {
        if (playersList == null) playersList = new List<Player>();
        // TODO: This operates on the logic that the same channel gets called to add/remove
        // Must refactor when we have a dedicated remove player event channel
        if (playersList.Contains(player))
        {
            playersList.Remove(player);
            return;
        }
        playersList.Add(player);
    }

    public void ClearPlayers()
    {
        playersList.Clear();
    }
}
