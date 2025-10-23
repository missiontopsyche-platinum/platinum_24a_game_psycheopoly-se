using UnityEngine;
using UnityEngine.UI;
using Logging;

public class PlayerPanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] private TurnStartedEventChannel turnStartedChannel;

    [Header("Data Source")] // TODO: needs to be decoupled, currently looking for a solution
    [SerializeField] private PlayerManager playerManager;

    [Header("UI Elements")]
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text playerMoneyText;

    private void OnEnable()
    {
        Subscribe(turnStartedChannel, DisplayCurrentPlayer);
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
        var player = playerManager?.GetPlayer(currentPlayerId);

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
}
