using System.Text;
using Events.EventDataStructures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnHoverUI : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private SpaceHoverEventChannel spaceHoverEventChannel;
    [SerializeField] private BooleanEventChannel onSpaceExitEventChannel;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Art")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image artworkImage;

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
            if (e.spaceData is PropertySpaceData)
            {
                bodyText.gameObject.SetActive(true);
            }
            else
            {
                bodyText.gameObject.SetActive(true);

                var sb = new StringBuilder();
                foreach (var line in e.spaceInformation)
                    sb.AppendLine(line);

                bodyText.text = sb.ToString();
            }
        }

        //binds the art from SO
        SetImage(iconImage, e.smallIcon);
        SetImage(artworkImage, e.artwork);

        //hide rows unless set
        SetRow(costValueText, false);
        SetRow(rentValueText, false);
        SetRow(ownerValueText, false);

        //Property binding 
        if (e.spaceData is PropertySpaceData property)
        {
            var owner = property.GetOwner();
            bool isOwned = owner != null;

            //keep cost
            if (costValueText != null)
            {
                costValueText.text = property.buyPrice.ToString();
                SetRow(costValueText, true);
            }

            // rent
            if (rentValueText != null)
            {
                int level = property.GetCurrentUpgradeLevel();

                rentValueText.text = property.researchFundingValues[level].ToString();
                SetRow(rentValueText, true);
            }

            //required
            if (bodyText != null)
            {
                string ownedByLine = $"Owned By: {(isOwned ? owner.GetPName() : "Unowned")}";

                // You don't currently have a mortgaged flag in the code you posted.
                // So for now, this can only be Owned vs Unowned.
                string statusLine = $"Ownership Status: {(isOwned ? "Owned" : "Unowned")}";

                int lvl = property.GetCurrentUpgradeLevel(); // this exists at bottom of your PropertySpaceData
                string lvlDisplay = lvl == 5 ? "DISCOVERY" : lvl.ToString();
                string upgradeLine = $"Upgrade Level: {lvlDisplay}";

                bodyText.text =ownedByLine + "\n" + statusLine + "\n" + upgradeLine;
            }
        }

        Show();
    }

    private static void SetRow(TMP_Text valueText, bool visible)
    {
        if (valueText == null) return;
        //assumes the TMP is a child of the row
        valueText.transform.parent.gameObject.SetActive(visible);
    }

    private static void SetImage(Image img, Sprite sprite){
        if (img == null) 
        {
            return; 
        }
        img.sprite = sprite;
        img.enabled = sprite != null;
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
