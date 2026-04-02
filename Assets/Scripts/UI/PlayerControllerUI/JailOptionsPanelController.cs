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
/// This class is the initial UI-only, pre-wiring. Therefore, the built-out
/// game logic isn't implemented yet.
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

    [Header("Event Channels")]
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;
    [SerializeField] private UIActionEventChannel uiActionEventChannel;

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

        if (activationEvent.Context is not JailActivationContext context)
        {
            Logger.Error("JailOptionsPanelController.OnUIActivationEvent",
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
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);

    }
}