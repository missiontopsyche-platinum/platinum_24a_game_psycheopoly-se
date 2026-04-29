using System;
using UnityEngine;
using UnityEngine.UI;
using Logging;
using System.Collections.Generic;

public class PlayerPanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] public TurnStartedEventChannel turnStartedChannel;

    [Header("Settings")] 
    [Tooltip("The rate (in seconds) at which the player information is updated")]
    [SerializeField] public float pollRate = 0.25f;

    [Header("UI Elements")]
    [SerializeField] public Text playerNameText;
    [SerializeField] public Text playerMoneyText;

    private int currentPlayerId;

    private void OnEnable()
    {
        Subscribe(turnStartedChannel, OnTurnStarted);
        
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

    private void Start()
    {
        InvokeRepeating(nameof(UpdatePlayerUI), pollRate, pollRate);
    }

    private void OnTurnStarted(TurnStartedEvent tse)
    {
        currentPlayerId = tse.playerId;

        var player = PlayerManager.GetInstance().GetPlayer(currentPlayerId);
        Logging.Logger.Info("PlayerPanelController.DisplayCurrentPlayer",
            $"Current Player: {player.GetPName()} with ${player.GetMoney()}",
            LogCategory.UI,
            this);
    }

    private void UpdatePlayerUI()
    {
        var player = PlayerManager.GetInstance().GetPlayer(currentPlayerId);

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
    }
}
