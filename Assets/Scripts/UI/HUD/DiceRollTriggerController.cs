using UnityEngine;
using UnityEngine.UI;
using Logging;

public class DiceRollTriggerController : UIPanelBase
{
    [Header("Event Channels")]
    [SerializeField] public BooleanEventChannel diceRollPannelChannel;


    [Header("UI Elements")]
    [SerializeField] public Button diceRollTriggerButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diceRollTriggerButton?.onClick.AddListener(OnDiceRollTriggerClicked);

        Logging.Logger.Trace("TurnPanelController.OnEnable",
            "Turn panel is now enabled.",
            LogCategory.UI,
            this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        diceRollTriggerButton?.onClick.RemoveListener(OnDiceRollTriggerClicked);
        ClearSubscriptions();
        Logging.Logger.Trace("TurnPanelController.OnDisable",
            "Turn panel is now disabled.",
            LogCategory.UI,
            this);
    }

    public void OnDiceRollTriggerClicked()
    {
        diceRollPannelChannel?.RaiseEvent(true);

        Logging.Logger.Info("DiceRollTrigger.OnDiceRollTriggerClicked",
            "Move button clicked.",
            LogCategory.UI,
            this);
    }
}
