using System;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;
using TMPro;
using UnityEngine;

public class BankruptcyNotificationUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text playerNameText;

    [Header("Event Channels")] 
    [SerializeField] private UIActivationEventChannel activationEventChannel;
    [SerializeField] private UIActionEventChannel actionEventChannel;

    private Action currentOnAcknowledged = null;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        activationEventChannel.Subscribe(OnActivation);
    }

    private void OnDisable()
    {
        activationEventChannel.Unsubscribe(OnActivation);
    }

    private void OnActivation(UIActivationEvent uiae)
    {
        if (uiae.UIType == UIType.BankruptcyNotification)
        {
            if (uiae.Context is BankruptcyNotificationContext bnc)
            {
                currentOnAcknowledged = bnc.onAcknowledged;
                Show(bnc.player.GetPName());
            }
        }
    }

    private void Show(String playerName)
    {
        playerNameText.text = playerName;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        actionEventChannel.RaiseEvent(new UIActionEvent(
            UIType.BankruptcyNotification, 
            new BankruptcyAcknowledgement(currentOnAcknowledged)));
        currentOnAcknowledged = null;
    }
}
