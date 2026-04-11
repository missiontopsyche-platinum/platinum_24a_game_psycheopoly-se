using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Logging;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;

public class PropertyManagementButtonController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;
    [SerializeField] private TurnStartedEventChannel turnStartedChannel;
    [SerializeField] private PlayerEventChannel addPlayerEventChannel;

    [Header("UI Elements")]
    [SerializeField] private Button propertyManagementButton;

    private readonly List<Player> playersList = new();
    private int currentPlayerId = -1;

    private void OnEnable()
    {
        Subscribe(turnStartedChannel, OnTurnStarted);
        Subscribe(addPlayerEventChannel, AddPlayer);

        if (propertyManagementButton != null)
            propertyManagementButton.onClick.AddListener(OnPropertyManagementClicked);

        Logging.Logger.Trace(
            "PropertyManagementButtonController.OnEnable",
            "Property management button controller enabled.",
            LogCategory.UI,
            this
        );
    }

    private void OnDisable()
    {
        if (propertyManagementButton != null)
            propertyManagementButton.onClick.RemoveListener(OnPropertyManagementClicked);

        ClearSubscriptions();

        Logging.Logger.Trace(
            "PropertyManagementButtonController.OnDisable",
            "Property management button controller disabled.",
            LogCategory.UI,
            this
        );
    }

    private void OnTurnStarted(TurnStartedEvent turnStartedEvent)
    {
        if (turnStartedEvent == null)
        {
            Logging.Logger.Warn(
                "PropertyManagementButtonController.OnTurnStarted",
                "TurnStartedEvent is null.",
                LogCategory.UI,
                this
            );
            return;
        }

        currentPlayerId = turnStartedEvent.playerId;
    }

    private void AddPlayer(Player player)
    {
        if (player == null)
            return;

        if (playersList.Contains(player))
            return;

        playersList.Add(player);
    }

    private Player GetCurrentPlayer()
    {
        return playersList.Find(player => player.GetId() == currentPlayerId);
    }

    public void OnPropertyManagementClicked()
    {
        Player currentPlayer = GetCurrentPlayer();

        if (currentPlayer == null)
        {
            Logging.Logger.Warn(
                "PropertyManagementButtonController.OnPropertyManagementClicked",
                $"Could not find current player for playerId {currentPlayerId}.",
                LogCategory.UI,
                this
            );
            return;
        }

        uiActivationEventChannel?.RaiseEvent(
            new UIActivationEvent(
                UIType.PropertyManagement,
                new PropertyManagementActivationContext(currentPlayer)
            )
        );

        Logging.Logger.Info(
            "PropertyManagementButtonController.OnPropertyManagementClicked",
            $"Opened property management for player {currentPlayer.GetPName()}.",
            LogCategory.UI,
            this
        );
    }
}