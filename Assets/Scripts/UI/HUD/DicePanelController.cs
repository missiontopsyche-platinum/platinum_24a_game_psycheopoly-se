using UnityEngine;
using UnityEngine.UI;
using Logging;

public class DicePanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] private DiceRolledEventChannel diceRolledChannel;

    [Header("UI Elements")]
    [SerializeField] private Text dice1RolledText;
    [SerializeField] private Text dice2RolledText;
    [SerializeField] private Text diceTotalText;// Probably don't need this

    private void OnEnable()
    {
        Subscribe(diceRolledChannel, DisplayDiceRoll);
        Logging.Logger.Trace("DicePanelController.OnEnable",
            "Dice panel is now enabled.",
            LogCategory.UI,
            this);
    }

    private void OnDisable()
    {
        ClearSubscriptions();
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
                $"Event: {nameof(diceRolledEvent)} is null",
                LogCategory.UI,
                this);
            return;
        }

        SetTextSafe(dice1RolledText, $"Die One: {diceRolledEvent.dieOne}");
        SetTextSafe(dice2RolledText, $"Die Two: {diceRolledEvent.dieTwo}");
        SetTextSafe(diceTotalText, $"Total: {diceRolledEvent.totalRoll}");
    }
}