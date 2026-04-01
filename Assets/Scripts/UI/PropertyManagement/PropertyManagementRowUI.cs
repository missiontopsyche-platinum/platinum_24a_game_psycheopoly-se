using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Events.EventDataStructures;

public class PropertyManagementRowUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text propertyNameText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text upgradeStateText;
    [SerializeField] private TMP_Text mortgageStateText;
    [SerializeField] private TMP_Text costText;

    [Header("Button References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button downgradeButton;
    [SerializeField] private Button mortgageButton;
    [SerializeField] private Button unmortgageButton;

    [Header("Event Channels")]
    [SerializeField] private UpgradeRequestEventChannel upgradeRequestChannel;

    private Player player;
    private OwnableSpaceData property;
    private PropertyManagementUIController parentUI;

    public void Initialize(Player owningPlayer, OwnableSpaceData propertyData, PropertyManagementUIController ui)
    {
        player = owningPlayer;
        property = propertyData;
        parentUI = ui;

        HookButtons();
        RefreshRow();
    }

    private void HookButtons()
    {
         if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (downgradeButton != null)
        {
            downgradeButton.onClick.RemoveAllListeners();
            downgradeButton.onClick.AddListener(OnDowngradeClicked);
        }

        if (mortgageButton != null)
        {
            mortgageButton.onClick.RemoveAllListeners();
            mortgageButton.onClick.AddListener(OnMortgageClicked);
        }

        if (unmortgageButton != null)
        {
            unmortgageButton.onClick.RemoveAllListeners();
            unmortgageButton.onClick.AddListener(OnUnmortgageClicked);
        }
    }

    public void RefreshRow()
    {
        if (property == null)
            return;

        if (propertyNameText != null)
            propertyNameText.text = property.Name;

        if (typeText != null)
            typeText.text = property.Type.ToString();

        if (mortgageStateText != null)
            mortgageStateText.text = property.isMortgaged ? "Mortgaged" : "Active";

        if (property is PropertySpaceData streetProperty)
        {
            if (upgradeStateText != null)
                upgradeStateText.text = $"Upgrade Level: {streetProperty.GetCurrentUpgradeLevel()}";

            if (costText != null)
                costText.text = $"Upgrade: ${streetProperty.GetNextUpgradeCost()}   Mortgage: ${property.collaborationValue}";
        }
        else
        {
            if (upgradeStateText != null)
                upgradeStateText.text = "Upgrade Level: N/A";

            if (costText != null)
                costText.text = $"Mortgage: ${property.collaborationValue}";
        }

        RefreshButtonStates();
    }

    private void RefreshButtonStates()
    {
        if (upgradeButton != null)
            upgradeButton.interactable = CanUpgrade();

        if (downgradeButton != null)
            downgradeButton.interactable = CanDowngrade();

        if (mortgageButton != null)
            mortgageButton.interactable = CanMortgage();

        if (unmortgageButton != null)
            unmortgageButton.interactable = CanUnmortgage();
    }

    private bool CanUpgrade()
    {
        if (player == null || property is not PropertySpaceData streetProperty)
            return false;

        return player.GetValidUpgradableProperties().Contains(streetProperty);
    }

    private bool CanDowngrade()
    {
        if (property is not PropertySpaceData streetProperty)
            return false;

        return streetProperty.CanDowngrade() && !streetProperty.isMortgaged;
    }

    private bool CanMortgage()
    {
        if (player == null || property == null)
            return false;

        return player.GetMortgageableProperties().Contains(property);
    }

    private bool CanUnmortgage()
    {
        if (player == null || property == null)
            return false;

        return player.GetMortgagedProperties().Contains(property)
               && player.CanAfford(property.mortgagePayoffValue);
    }

    private void OnUpgradeClicked()
    {
        if (player == null || property is not PropertySpaceData streetProperty)
            return;

        upgradeRequestChannel?.RaiseEvent(new UpgradeRequestEvent(player, streetProperty));
    }

    private void OnDowngradeClicked()
    {
        if (property is not PropertySpaceData streetProperty)
            return;

        if (streetProperty.TryDowngrade())
        {
            parentUI?.RefreshUI();
        }
    }

    private void OnMortgageClicked()
    {
        if (player == null || property == null)
            return;

        if (player.MortgageProperty(property))
        {
            parentUI?.RefreshUI();
        }
    }

    private void OnUnmortgageClicked()
    {
        if (player == null || property == null)
            return;

        if (player.UnmortgageProperty(property))
        {
            parentUI?.RefreshUI();
        }
    }
}