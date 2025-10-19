using UnityEngine;
using UnityEngine.UI;
using Logging;

public class TurnPanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] private TurnStartedEventChannel turnStartedChannel;

    [Header("UI Elements")]
    [SerializeField] private Text turnNumberText;

    private void OnEnable()
    {
        Subscribe(turnStartedChannel, DisplayCurrentTurn);
        Logging.Logger.Trace("TurnPanelController.OnEnable",
            "Turn panel is now enabled.",
            LogCategory.UI,
            this);
    }

    private void OnDisable()
    {
        ClearSubscriptions();
        Logging.Logger.Trace("TurnPanelController.OnDisable",
            "Turn panel is now disabled.",
            LogCategory.UI,
            this);
    }

    public void DisplayCurrentTurn(TurnStartedEvent turnStartedEvent)
    {
        if (turnStartedEvent == null)
        {
            Logging.Logger.Warn("PlayerPanelController.DisplayCurrentPlayer",
                $"Event: {nameof(turnStartedEvent)} is null",
                LogCategory.UI,
                this);
            return;
        }

        SetTextSafe(turnNumberText, $"Turn: {turnStartedEvent.turnNum}");
    }
}
