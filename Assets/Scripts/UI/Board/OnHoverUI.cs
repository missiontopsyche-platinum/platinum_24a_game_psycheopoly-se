using System.Collections;
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

    [Header("Animation Settings")] 
    [SerializeField] private float delayTime = 0.5f;
    [SerializeField] private AnimationCurve easeCurve;
    [SerializeField] private float fadeTime = 1f;

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
    
    private float currentTime = 0f;
    private bool isHidden = true;
    
    private void Awake()
    {
        StartCoroutine(Hide());
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
            StartCoroutine(Hide());
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

        StopAllCoroutines();
        StartCoroutine(Show());
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
        StopAllCoroutines();
        StartCoroutine(Hide());
    }

    private IEnumerator Show()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (isHidden)
        {
            yield return new WaitForSeconds(delayTime);
            isHidden = false;
        }

        while (currentTime < fadeTime)
        {
            canvasGroup.alpha = easeCurve.Evaluate(currentTime / fadeTime);
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator Hide()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        while (currentTime > 0f)
        {
            canvasGroup.alpha = easeCurve.Evaluate(currentTime / fadeTime);
            yield return new WaitForEndOfFrame();
            currentTime -= Time.deltaTime;
        }
        
        canvasGroup.alpha = 0f;
        isHidden = true;
    }
}
