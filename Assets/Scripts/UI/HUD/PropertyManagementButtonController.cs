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
        Debug.Log("PropertyManagementButtonController OnEnable");

        Subscribe(turnStartedChannel, OnTurnStarted);
        Subscribe(addPlayerEventChannel, AddPlayer);

        if (propertyManagementButton != null)
        {
            propertyManagementButton.onClick.RemoveListener(OnPropertyManagementClicked);
            propertyManagementButton.onClick.AddListener(OnPropertyManagementClicked);
        }
    }

    private void OnDisable()
    {
        if (propertyManagementButton != null)
            propertyManagementButton.onClick.RemoveListener(OnPropertyManagementClicked);

        ClearSubscriptions();
    }

    private void OnTurnStarted(TurnStartedEvent turnStartedEvent)
    {
        Debug.Log("OnTurnStarted fired");

        if (turnStartedEvent == null)
        {
            Debug.Log("TurnStartedEvent is null");
            return;
        }

        currentPlayerId = turnStartedEvent.playerId;
        Debug.Log("Current player id set to: " + currentPlayerId);
    }

    private void AddPlayer(Player player)
    {
        Debug.Log("AddPlayer fired for: " + (player == null ? "null" : player.GetPName()));

        if (player == null)
            return;

        if (!playersList.Contains(player))
            playersList.Add(player);

        Debug.Log("Player list count: " + playersList.Count);
    }

    private Player GetCurrentPlayer()
    {
        Player player = playersList.Find(p => p.GetId() == currentPlayerId);
        Debug.Log("GetCurrentPlayer result: " + (player == null ? "null" : player.GetPName()));
        return player;
    }

    public void OnPropertyManagementClicked()
    {
        Debug.Log("Property button clicked");
        Debug.Log("Current player id when clicked: " + currentPlayerId);
        Debug.Log("Players count when clicked: " + playersList.Count);

        Player currentPlayer = GetCurrentPlayer();

        if (currentPlayer == null)
        {
            Debug.LogWarning("Could not find current player for playerId " + currentPlayerId);
            return;
        }

        uiActivationEventChannel?.RaiseEvent(
            new UIActivationEvent(
                UIType.PropertyManagement,
                new PropertyManagementActivationContext(currentPlayer)
            )
        );

        Debug.Log("Raised PropertyManagement activation event");
    }
}