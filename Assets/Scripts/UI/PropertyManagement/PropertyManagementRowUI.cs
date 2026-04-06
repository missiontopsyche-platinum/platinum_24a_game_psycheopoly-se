using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Events.EventChannelTypes;
using Events.EventDataStructures.UI;

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
    [SerializeField] private UIActionEventChannel uiActionEventChannel;

    private Player player;
    private OwnableSpaceData property;
    private bool isDebtResolutionMode;

    public void Initialize(Player owningPlayer, OwnableSpaceData propertyData, bool debtMode)
    {
        player = owningPlayer;
        property = propertyData;
        isDebtResolutionMode = debtMode;

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

        propertyNameText.text = property.Name;
        typeText.text = property.Type.ToString();
        mortgageStateText.text = property.isMortgaged ? "Mortgaged" : "Active";

        if (property is PropertySpaceData streetProperty)
        {
            upgradeStateText.text = $"Upgrade Level: {streetProperty.GetCurrentUpgradeLevel()}";
            costText.text = $"Upgrade: ${streetProperty.GetNextUpgradeCost()}   Mortgage: ${property.collaborationValue}";
        }
        else
        {
            upgradeStateText.text = "Upgrade Level: N/A";
            costText.text = $"Mortgage: ${property.collaborationValue}";
        }

        RefreshButtonStates();
    }

    private void RefreshButtonStates()
    {
        if (isDebtResolutionMode)
        {
            if (upgradeButton != null)
                upgradeButton.interactable = false;

            if (unmortgageButton != null)
                unmortgageButton.interactable = false;

            if (downgradeButton != null)
                downgradeButton.interactable = CanDowngrade();

            if (mortgageButton != null)
                mortgageButton.interactable = CanMortgage();

            return;
        }

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
        if (isDebtResolutionMode) return;
        if (property is not PropertySpaceData streetProperty) return;

        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.PropertyUpgradeSelected,
                new PropertyUpgradeContext(streetProperty)));
    }

    private void OnDowngradeClicked()
    {
        if (property is not PropertySpaceData streetProperty) return;

        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.PropertyManagement,
                new PropertyDowngradeContext(streetProperty)));
    }

    private void OnMortgageClicked()
    {
        if (property == null) return;

        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.MortgagePropertySelected,
                new MortgagePropertyContext(player, property)));
    }

    private void OnUnmortgageClicked()
    {
        if (isDebtResolutionMode) return;
        if (property == null) return;

        uiActionEventChannel?.RaiseEvent(
            new UIActionEvent(
                UIType.PropertyManagement,
                new UnmortgagePropertyContext(property)));
    }
}