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

    [Header("Info Grid")]
    [SerializeField] private TMP_Text costValueText;
    [SerializeField] private TMP_Text rentValueText;
    [SerializeField] private TMP_Text ownerValueText;

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
        if (e == null || e.spaceData == null)
        {
            Hide();
            return;
        }

        //Title
        if (titleText != null)
            titleText.text = e.spaceData.GetShortName();

        //keep bodyText for extra lines
        if (bodyText != null)
        {
            var sb = new StringBuilder();
            foreach (var line in e.spaceInformation)
                sb.AppendLine(line);
            bodyText.text = sb.ToString();
        }

        //hide rows unless set
        SetRow(costValueText, false);
        SetRow(rentValueText, false);
        SetRow(ownerValueText, false);

        //Property binding 
        if (e.spaceData is PropertySpaceData property)
        {
            //cost
            if (costValueText != null)
            {
                costValueText.text = property.buyPrice.ToString();
                SetRow(costValueText, true);
            }

            // rent
            if (rentValueText != null)
            {
                rentValueText.text = property.collaborationValue.ToString();
                SetRow(rentValueText, true);
            }

            //owner
            if (ownerValueText != null)
            {
                ownerValueText.text = "Unowned";
                SetRow(ownerValueText, true);
            }
        }

        Show();
    }

    private static void SetRow(TMP_Text valueText, bool visible)
    {
        if (valueText == null) return;
        // assumes the TMP is a child of the row (CostRow/RentRow/OwnerRow)
        valueText.transform.parent.gameObject.SetActive(visible);
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
