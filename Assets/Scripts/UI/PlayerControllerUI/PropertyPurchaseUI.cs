using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyPurchaseUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image propertyColorPanel;
    [SerializeField] private TMP_Text propertyNameText;
    [SerializeField] private TMP_Text propertyBuyPriceText;
    [SerializeField] private TMP_Text propertyRentsText;
    [SerializeField] private TMP_Text propertyUpgradesText;
    [SerializeField] private Button acceptButton;

    [Header("Event Channels")] 
    [SerializeField] private UIActivationEventChannel uiActivationEventChannel;
    [SerializeField] private UIActionEventChannel uiActionEventChannel;

    private PropertySpaceData currentProperty;

    public void OnPurchaseClick() => FirePurchaseAction(true);
    public void OnDeclineClick() => FirePurchaseAction(false);

    private void FirePurchaseAction(bool isPurchasing)
    {
        if (!currentProperty) return;
        
        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.PropertyPurchase,
                new PurchaseActionContext(
                    isPurchasing,
                    currentProperty)));
    }
}
