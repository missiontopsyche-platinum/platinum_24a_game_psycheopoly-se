using System.Collections;
using Logging;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures;
using Events.EventDataStructures.UI;
using Logger = Logging.Logger;

public class DiceRollPanelController : MonoBehaviour
{
    [Header ("Event Channels")]
    [SerializeField] private DiceRolledEventChannel diceRolledChannel;
    [SerializeField] private BooleanEventChannel pieceMoveCompletedChannel;
    [SerializeField] private BooleanEventChannel rollDiceRequestedChannel;
    [SerializeField] private UIActivationEventChannel uiActivationChannel;

    [Header("UI")]
    [SerializeField] private DiceFaceView dieOneView;
    [SerializeField] private DiceFaceView dieTwoView;
    [SerializeField] private Text totalText;

    [Header("Dice")]
    [SerializeField] private DiceManager diceManager;
    [Header("Optional")]
    [SerializeField] private Button rollButton;      
    [SerializeField] private bool hideUntilFirstRoll = false;
    [SerializeField] private float timeUntilDisableAfterRoll = 2f;

    private void Awake()
    {
        if (totalText != null) totalText.text = "Total: -";
        if (rollButton != null) rollButton.onClick.AddListener(OnRollClicked);
        diceRolledChannel?.Subscribe(OnDiceRolled);
        pieceMoveCompletedChannel?.Subscribe(HideUI);
        uiActivationChannel?.Subscribe(OnUIActivationEvent);
        if (hideUntilFirstRoll) HideUI(true);
        
    }

    private void OnDestroy()
    {
        if (rollButton != null) rollButton.onClick.RemoveListener(OnRollClicked);
        diceRolledChannel?.Unsubscribe(OnDiceRolled);
        pieceMoveCompletedChannel?.Unsubscribe(HideUI);
        uiActivationChannel?.Unsubscribe(OnUIActivationEvent);
    }


    private void OnRollClicked()
    {
        Logger.Debug("DiceRollPanel.OnRollClicked", "Roll clicked!", LogCategory.Gameplay, this);
        diceManager.RollDice();
           
    }

    private void OnDiceRolled(DiceRolledEvent e)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (dieOneView != null) dieOneView.SetValue(e.dieOne);
        if (dieTwoView != null) dieTwoView.SetValue(e.dieTwo);
        if (totalText != null)
        {
            StartCoroutine(AnimateTotalText(e.totalRoll));
        }
    }

    private IEnumerator AnimateTotalText(int total)
    {
        string baseText = "Total: ";
        totalText.text = baseText;
        for (int i = 0; i < 20; i++)
            yield return null;
        totalText.text = baseText + total;
    }

    private void HideUI(bool pieceMoveCompleted)
    {
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
        gameObject.GetComponent<CanvasGroup>().interactable = false;
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private void OnUIActivationEvent(UIActivationEvent uiae)
    {
        if (uiae == null || uiae.UIType != UIType.DiceRoll)
            return;

        Logger.Debug("DiceRollPanelController.OnUIActivationEvent",
            "Dice Roll Panel Launching.",
            LogCategory.UI);

        if (uiae.UIType == UIType.DiceRoll)
        {
            CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
