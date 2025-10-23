using UnityEngine;
using UnityEngine.UI;
using Logging;

public class DicePanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] private DiceRolledEventChannel diceRolledChannel;
    [SerializeField] private RollDiceRequestedEventChannel diceRolledRequestedChannel;

    [Header("UI Elements")]
    [SerializeField] private Text dice1RolledText;
    [SerializeField] private Text dice2RolledText;
    [SerializeField] private Text diceTotalText;// Probably don't need this

    [Header("UI Controls")]
    [SerializeField] private Button rollDiceButton;
    private void OnEnable()
    {
        if (rollDiceButton == null)
        {
            Logging.Logger.Warn("DicePanelController.OnEnable",
                "rollDiceButton is null",
                LogCategory.UI,
                this);
            return;
        }
        Subscribe(diceRolledChannel, DisplayDiceRoll);
        rollDiceButton.onClick.AddListener(RollDiceClicked);
        Logging.Logger.Trace("DicePanelController.OnEnable",
                "Dice panel is now enabled.",
                LogCategory.UI,
                this);
    }

    private void OnDisable()
    {
        ClearSubscriptions();

        if (rollDiceButton != null) rollDiceButton.onClick.RemoveListener(RollDiceClicked);

        Logging.Logger.Trace("DicePanelController.OnDisable",
            "Dice panel is now disabled.",
            LogCategory.UI,
            this);
    }

    public void DisplayDiceRoll(DiceRolledEvent diceRolledEvent)
    {
        if (diceRolledEvent == null)
        {
            Logging.Logger.Warn("DicePanelController.DisplayDiceRoll",
                $"DiceRolledEvent is null",
                LogCategory.UI,
                this);
            return;
        }

        SetTextSafe(dice1RolledText, $"Die One: {diceRolledEvent.dieOne}");
        SetTextSafe(dice2RolledText, $"Die Two: {diceRolledEvent.dieTwo}");
        SetTextSafe(diceTotalText, $"Total: {diceRolledEvent.totalRoll}");
    }

    public void RollDiceClicked()
    {
        if (diceRolledRequestedChannel == null)
        {
            Logging.Logger.Error("DicePanelController.RollDiceClicked",
                $"DiceRolledRequestedChannel is null",
                LogCategory.UI,
                this);
            return;
        }
        diceRolledRequestedChannel.RaiseEvent(true);
    }
}