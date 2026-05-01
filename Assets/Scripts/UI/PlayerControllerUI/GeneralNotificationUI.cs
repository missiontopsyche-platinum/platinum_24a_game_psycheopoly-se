using System;
using System.Collections;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;
using Logging;
using TMPro;
using UnityEngine;

public class GeneralNotificationUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text notificationTitle;
    [SerializeField] private TMP_Text notificationText;
    
    [Header("Configuration")]
    [SerializeField] private float automaticWaitTime = 1;

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
        if (uiae.UIType == UIType.GeneralNotification)
        {
            if (uiae.Context is GeneralNotificationContext gnc)
            {
                currentOnAcknowledged = gnc.onAcknowledged;
                if (gnc.isAI) 
                    StartCoroutine(AutoCloseTimer());
                Show(gnc);
            }
        }
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(automaticWaitTime);
        Hide();
    }

    private void Show(GeneralNotificationContext gnc)
    {
        playerNameText.text = gnc.player.GetPName();
        notificationTitle.text = gnc.notificationTitle;
        notificationText.text = gnc.notificationText;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Logging.Logger.Info("GeneralNotificationUI.Show",
            $"[{playerNameText.text}] {notificationTitle.text}: {notificationText.text}",
            LogCategory.UI);
    }

    public void Hide()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        actionEventChannel.RaiseEvent(new UIActionEvent(
            UIType.GeneralNotification, 
            new GeneralAcknowledgement(currentOnAcknowledged)));
        currentOnAcknowledged = null;
    }
}
