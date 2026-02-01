using System.Text;
using Events.EventDataStructures;
using TMPro;
using UnityEngine;

public class OnHoverUI : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private SpaceHoverEventChannel spaceHoverEventChannel;
    [SerializeField] private BooleanEventChannel onSpaceExitEventChannel;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    private void Awake()
    {
        Hide();
    }

    private void OnEnable()
    {
        spaceHoverEventChannel?.Subscribe(OnSpaceHover);
        onSpaceExitEventChannel?.Subscribe(OnSpaceExit);
    }

    private void OnDisable()
    {
        spaceHoverEventChannel?.Unsubscribe(OnSpaceHover);
        onSpaceExitEventChannel?.Unsubscribe(OnSpaceExit);
    }

    private void OnSpaceHover(SpaceHoverEvent e)
    {
        if (e == null)
        {
            Hide();
            return;
        }

        //One panel at a time
        if (titleText != null)
            titleText.text = e.spaceName;

        if (bodyText != null)
        {
            var sb = new StringBuilder();
            foreach (var line in e.spaceInformation)
                sb.AppendLine(line);
            bodyText.text = sb.ToString();
        }

        Show();
    }

    private void OnSpaceExit(bool _)
    {
        Hide();
    }

    private void Show()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void Hide()
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
