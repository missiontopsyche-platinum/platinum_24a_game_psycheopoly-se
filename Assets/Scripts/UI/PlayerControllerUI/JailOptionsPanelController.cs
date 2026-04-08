using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;
using Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Controls UI Panel for the Jail Options for HUMAN players
/// Renders jail decisions for
/// - roll for escape
/// - pay fine
/// - use get out of jail free card
/// This class is UI-only. It displays jail state, raises UI action events,
/// and does NOT contain the underlying jail game logic.
/// </summary>
public class JailOptionsPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text fineText;

    [SerializeField] private Button rollButton;
    [SerializeField] private Button payFineButton;
    [SerializeField] private Button useCardButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private TMP_Text payFineButtonText;
    [SerializeField] private TMP_Text useCardButtonText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Event Channels")]
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;
    [SerializeField] private UIActionEventChannel uiActionEventChannel;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Logging.Logger.Error("JailOptionsPanelController.Awake",
                "CanvasGroup is missing from JailOptionsPanelController.",
                LogCategory.UI,
                this);
            return;
        }

        Hide();
    }

    private void OnEnable()
    {
        uiActivationEventChannel?.Subscribe(OnUIActivationEvent);
    }

    private void OnDisable()
    {
        uiActivationEventChannel?.Unsubscribe(OnUIActivationEvent);
    }

    private void OnUIActivationEvent(UIActivationEvent activationEvent)
    {
        if (activationEvent == null || activationEvent.UIType != UIType.JailOptions)
            return;

        JailActivationContext context = activationEvent.Context as JailActivationContext;
        if (context == null)
        {
            Logging.Logger.Error("JailOptionsPanelController.OnUIActivationEvent",
                $"Expected {nameof(JailActivationContext)} but got {activationEvent.Context?.GetType().Name}",
                LogCategory.UI,
                this);
            return;
        }

        titleText.text = $"{context.PlayerName} is in Jail";
        statusText.text = $"Jail Turn: {context.JailTurns}/{context.MaxJailTurns}";
        fineText.text = $"Pay Fine: ${context.FineAmount}";

        if (payFineButton != null)
            payFineButton.interactable = context.CanAffordFine;

        if (payFineButtonText != null)
            payFineButtonText.text = context.CanAffordFine
                ? $"Pay ${context.FineAmount}"
                : $"Cannot Afford ${context.FineAmount}";

        if (useCardButton != null)
            useCardButton.interactable = context.HasGetOutOfJailCard;

        if (useCardButtonText != null)
            useCardButtonText.text = context.HasGetOutOfJailCard
                ? "Use Jail Card"
                : "No Jail Card";

        Show();
    }

    public void OnRollForEscapeClicked()
    {
        FireAction(JailChoice.RollForEscape);
    }

    public void OnPayFineClicked()
    {
        FireAction(JailChoice.PayFine);
    }

    public void OnUseCardClicked()
    {
        FireAction(JailChoice.UseCard);
    }

    public void OnCloseClicked()
    {
        Hide();
    }

    private void FireAction(JailChoice choice)
    {
        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.JailOptions,
                new JailActionContext(choice)));


        Hide();
    }

    private void Show()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void Hide()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}