using UnityEngine;
using UnityEngine.UI;
using Logging;

public class TurnPanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] public TurnStartedEventChannel turnStartedChannel;
    [SerializeField] public BooleanEventChannel turnEndedChannel;

    [Header("UI Elements")]
    [SerializeField] public Text turnNumberText;
    [SerializeField] public Button endTurnButton;

    private void OnEnable()
    {
        Subscribe(turnStartedChannel, DisplayCurrentTurn);
        endTurnButton?.onClick.AddListener(OnEndTurnClicked);

        Logging.Logger.Trace("TurnPanelController.OnEnable",
            "Turn panel is now enabled.",
            LogCategory.UI,
            this);
    }

    private void OnDisable()
    {
        endTurnButton?.onClick.RemoveListener(OnEndTurnClicked);
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

    public void OnEndTurnClicked()
    {
        Logging.Logger.Info("TurnPanelController.OnEndTurnClicked",
            "End Turn button clicked.",
            LogCategory.UI,
            this);
        try
        {
            turnEndedChannel.RaiseEvent(true);
        }
        catch (MissingComponentException ex)
        {
            Logging.Logger.Warn("TurnPanelController.OnEndTurnClicked",
                ex.Message,
                LogCategory.UI,
                this);
        }
    }
}
