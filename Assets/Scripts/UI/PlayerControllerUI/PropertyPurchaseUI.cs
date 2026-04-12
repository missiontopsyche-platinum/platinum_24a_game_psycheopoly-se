using System;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;
using Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PropertyPurchaseUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image ownableColorPanel;
    [SerializeField] private TMP_Text ownableNameText;
    [SerializeField] private TMP_Text ownableByBuyPriceText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private RectTransform propertyTextCanvas;
    [SerializeField] private RectTransform instrumentTextCanvas;
    [SerializeField] private RectTransform planetTextCanvas;

    [Header("UI Elements/Property Text Elements")]
    [SerializeField] private TMP_Text propertyRentsText;
    [SerializeField] private TMP_Text propertyUpgradesText;

    [Header("UI Elements/Instrument Text Elements")]
    [SerializeField] private TMP_Text instrumentPricesText;

    [Header("Event Channels")]
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;
    [SerializeField] private UIActionEventChannel uiActionEventChannel;

    private OwnableSpaceData currentProperty;

    private CanvasGroup canvasGroup;

    public void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        Hide();
    }
    public void OnEnable()
    {
        uiActivationEventChannel.Subscribe(OnUIActivationEvent);
    }

    public void OnDestroy()
    {
        uiActivationEventChannel.Unsubscribe(OnUIActivationEvent);
    }

    private void OnUIActivationEvent(UIActivationEvent uiae)
    {
        
        if (uiae.UIType != UIType.PropertyPurchase) return;

        if (uiae.Context is PurchaseActivationContext context)
        {
            currentProperty = context.Property;

            // check the type of ownable to set the UI text panels
            if (context.Property is PropertySpaceData property)
            {
                SetPropertyText(property);
                propertyTextCanvas.gameObject.SetActive(true);
            } 
            else if (context.Property is InstrumentSpaceData instrument)
            {
                SetInstrumentText(instrument);
                propertyTextCanvas.gameObject.SetActive(true);
            } 
            else if (context.Property is PlanetSpaceData)
            {
                planetTextCanvas.gameObject.SetActive(true);
            } 
            else
            {
                Logging.Logger.Error("PropertyPurchaseUI.OnUIActivationEvent",
                    "Ownable Space Data passed as context is not a valid subclass.",
                    LogCategory.UI);
                return;
            }
            
            ownableColorPanel.color = context.Property.groupColor;
            ownableNameText.text = context.Property.spaceName;
            ownableByBuyPriceText.text = $"Buy Price: ${context.Property.buyPrice}";

            if (context.CanAfford)
            {
                acceptButton.interactable = true;
                buttonText.text = "Purchase";
            }
            else
            {
                acceptButton.interactable = false;
                buttonText.text = "Unable to Afford Purchase";
            }
            Show();
        }
        else
        {
            Logging.Logger.Error("PropertyPurchaseUI.OnUIActivationEvent",
                "UI context is incorrect for the UI Type targeted by event. " +
                $"Expected PurchaseActivationContext but received {uiae.Context.GetType()}.",
                LogCategory.UI);
        }
    }

    private void Show()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Logging.Logger.Error("PropertyPurchaseUI.Show",
                "Showing the UI",
                LogCategory.UI);
    }

    private void Hide()
    {
        
        planetTextCanvas.gameObject.SetActive(false);
        instrumentTextCanvas.gameObject.SetActive(false);
        propertyTextCanvas.gameObject.SetActive(false);
        
        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void SetPropertyText(PropertySpaceData property)
    {
        // the pattern here is to make the UI rendering consistent.
        // This is pretty brittle, and could use another look later on.
         propertyRentsText.text = $"${property.researchFundingValues[0]}\n" +
                                  $"${property.researchFundingValues[2]}\n" +
                                  $"${property.researchFundingValues[1]}\n" +
                                  $"${property.researchFundingValues[3]}\n" +
                                  $"${property.researchFundingValues[4]}\n" +
                                  $"${property.researchFundingValues[5]}\n" +
                                  "\n" +
                                  $"${property.collaborationValue}";
        propertyUpgradesText.text = $"${property.dataPointCost}" +
                                    "\n\n" +
                                    $"${property.dataPointCost}";
    }

    private void SetInstrumentText(InstrumentSpaceData instrument)
    {
         instrumentPricesText.text = $"${instrument.researchFundingLevels[0]}\n" +
                                     $"${instrument.researchFundingLevels[1]}\n" +
                                     $"${instrument.researchFundingLevels[2]}\n" +
                                     $"${instrument.researchFundingLevels[3]}\n";
    }

    public void OnPurchaseClick() => FirePurchaseAction(true);
    public void OnDeclineClick() => FirePurchaseAction(false);

    private void FirePurchaseAction(bool isPurchasing)
    {
        Hide();

        if (!currentProperty) return;
        
        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.PropertyPurchase,
                new PurchaseActionContext(
                    isPurchasing,
                    currentProperty)));

        
    }
}
