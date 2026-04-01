using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private Player player;
    private OwnableSpaceData property;

    public void Initialize(Player owningPlayer, OwnableSpaceData propertyData)
    {
        player = owningPlayer;
        property = propertyData;
        RefreshRow();
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
    }
}