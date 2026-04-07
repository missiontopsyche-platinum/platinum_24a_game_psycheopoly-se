using UnityEngine;
using UnityEngine.UI;
using Logging;

public class DicePanelController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] public DiceRolledEventChannel diceRolledChannel;
    [SerializeField] public BooleanEventChannel diceRolledRequestedChannel;

    [Header("UI Elements")]
    [SerializeField] public Text dice1RolledText;
    [SerializeField] public Text dice2RolledText;
    [SerializeField] public Text diceTotalText;// Probably don't need this

    [Header("UI Controls")]
    [SerializeField] public Button rollDiceButton;

    private void OnEnable()
    {
        if (!rollDiceButton)
            rollDiceButton = GetComponentInChildren<Button>(true);

        if (!rollDiceButton)
        {
            Logging.Logger.Warn("DicePanelController.OnEnable",
                "rollDiceButton is null",
                LogCategory.UI, 
                this);
        } else 
            rollDiceButton.onClick.AddListener(RollDiceClicked);
        
        Subscribe(diceRolledChannel, DisplayDiceRoll);
        
        Logging.Logger.Trace("DicePanelController.OnEnable",
                "Dice panel is now enabled.",
                LogCategory.UI,
                this);
    }
    
    private void OnDestroy()
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
        
        Logging.Logger.Debug("DicePanelController.DisplayDiceRoll",
            $"Die 1: {diceRolledEvent.dieOne}, Die 2: {diceRolledEvent.dieTwo}, Total: {diceRolledEvent.totalRoll}",
            LogCategory.Gameplay, this);

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