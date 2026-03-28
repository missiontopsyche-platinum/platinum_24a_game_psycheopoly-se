using Events.EventDataStructures;
using Assets.Scripts.Managers.Rent;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PropertySpaceData", menuName = "Board Spaces/Property Space")]
public class PropertySpaceData : OwnableSpaceData, IUpgradableTileInfo
{
    [Header("Property-Specific Values")]
    [SerializeField] public int[] researchFundingValues = new int[6];
    [SerializeField] public int dataPointCost;
    [SerializeField, Range(1.0f, 2.0f)] public float upgradeCostMultiplier = 1.0f; // upgrade cost is defined to not change by default
    [SerializeField, HideInInspector] private int[] upgradeCostByLevel;
    [SerializeField] private ColorGroup colorGroup;

    [Header("Property Event Channels")] 
    [SerializeField] public PurchaseUpgradeRequestEventChannel purchaseUpgradeRequestEventChannel;
    
    private int currentUpgradeLevel = 0;
    private int[] ints;
    private int upgradeCost;

    public PropertySpaceData(int[] ints, int upgradeCost)
    {
        this.ints = ints;
        this.upgradeCost = upgradeCost;
    }

    public int MaxUpgradeLevel =>
    researchFundingValues != null
        ? researchFundingValues.Length - 1
        : 0;
    public bool IsMaxed => currentUpgradeLevel >= MaxUpgradeLevel;
    public int[] UpgradeCostByLevel => upgradeCostByLevel;
    public bool CanUpgrade() => !IsMaxed && dataPointCost > 0 && !isMortgaged;

    private void OnEnable()
    {
        BuildUpgradeCostTable();
    }

    private void OnValidate()
    {
        BuildUpgradeCostTable();
    }

    public override void OnLanded(Player player)
    {
        base.OnLanded(player);
        if (owner == null) return;

        // If the property is mortaged, no OnLanded events should fire.
        if (isMortgaged) return; 

        /*if (owner.Equals(player))
        {
            // TODO remove
            // after double checking the rules, players can upgrade any properties they want at
            // the start of their turn, not when they land on a property. This should be "do nothing".
            
            // maybe we move this to GameManager so we can modify how this works
            // in a ruleset later on...
            
            // if the current upgrade level is less than DISCOVERY
            if (currentUpgradeLevel < 5)
            {
                // offer to upgrade space in UI in future
                purchaseUpgradeRequestEventChannel?.RaiseEvent(new PurchaseOwnableRequestEvent(
                    player,
                    this,
                    dataPointCost));
            } // else do nothing or inform player they cant upgrade anymore
        }
        else
        {
            // charge player from researchFundingValues at the current upgrade level
            int chargeAmount = researchFundingValues[currentUpgradeLevel];
            chargeOwnershipFeeEventChannel?.RaiseEvent(new ChargeOwnershipFeeEvent(
                player, owner,
                chargeAmount, this));
        }*/

        if (!owner.Equals(player))
        {
            int chargeAmount = researchFundingValues[currentUpgradeLevel];
            chargeOwnershipFeeEventChannel?.RaiseEvent(new ChargeOwnershipFeeEvent(
                player, owner,
                chargeAmount, this));
        }
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        var payload = base.OnHover();
        
        // current status info
        payload.AppendInformation(
            "Current Improvement: " + 
            currentUpgradeLevel switch
        {
            0 => "No Data Points",
            1 or 2 or 3 or 4 => $"{currentUpgradeLevel} Data Point(s)",
            5 => "DISCOVERY",
            _ => "INVALID UPGRADE LEVEL"
        });
        payload.AppendInformation(
            "Current Research Funding Cost: " + 
            $"${researchFundingValues[currentUpgradeLevel]}");
        
        payload.AppendInformation("============");
        // general info (prices, etc)
        for (int i = 0; i < researchFundingValues.Length; i++)
        {
            if (i == 0)
                payload.AppendInformation(
                    $"Research Funding Cost: ${researchFundingValues[0]}");
            else if (i == 5)
                payload.AppendInformation(
                    $"With DISCOVERY: ${researchFundingValues[5]}");
            else
                payload.AppendInformation(
                    $"With {i} Data Points: ${researchFundingValues[i]}");
        }
        payload.AppendInformation($"Cost per Data Point/DISCOVERY: ${dataPointCost}");
        
        // In the future, maybe we could indicate how many spaces of this color category the player owns.
        
        spaceHoverEventChannel?.RaiseEvent(payload);
        return payload;
    }

    /// <summary>
    /// Called by GameManager if Upgrade Request is approved, increments Upgrade Level by 1.
    /// </summary>
    public void UpgradeProperty()
    {
        currentUpgradeLevel += 1;
        this.VerifyMortagableStatus();
    }

    public int GetCurrentUpgradeLevel()
    {
        return currentUpgradeLevel;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade())
            return false;

        currentUpgradeLevel = Mathf.Clamp(currentUpgradeLevel + 1, 0, MaxUpgradeLevel);
        this.VerifyMortagableStatus();
        return true;
    }

    // For testing
    public void SetUpgradeLevel(int level)
    {
        currentUpgradeLevel = Mathf.Clamp(level, 0, MaxUpgradeLevel);
    }
    public void SetDataPointCost(int cost)
    {
        dataPointCost = cost;
    }
    public void SetUpgradeCostByLevel(int[] levels)
    {
        upgradeCostByLevel = levels;
    }
    public void SetResearchFundingValues(int[] values)
    {
        researchFundingValues = values;
    }

    public int GetUpgradeCostForLevel(int level)
    {
        if (level < 1 || level > MaxUpgradeLevel)
            return 0;

        return dataPointCost;
    }

    public int GetNextUpgradeCost()
    {
        if (IsMaxed)
            return 0;

        return GetUpgradeCostForLevel(currentUpgradeLevel + 1);
    }

    private static int RoundToNearestTens(int number)
    {
        double divided = number / 10.0;

        double rounded = Math.Round(divided);

        int result = (int)rounded * 10;

        return result;
    }

    private void BuildUpgradeCostTable()
    {
        int max = MaxUpgradeLevel;

        if (max <= 0 || dataPointCost <= 0)
        {
            upgradeCostByLevel = Array.Empty<int>();
            return;
        }

        upgradeCostByLevel = new int[max];
        for (int i = 0; i < max; i++)
            upgradeCostByLevel[i] = GetUpgradeCostForLevel(i);
    }

    private void VerifyMortagableStatus()
    {

        if (this.currentUpgradeLevel > 0)
        {
            this.isMortgageable = false;
            return;
        }
            

        this.isMortgageable = true;


    }

    public override TileType Type => TileType.Street;

    public override ColorGroup Group => colorGroup;

    public override int HouseCount => currentUpgradeLevel;

    public override int BaseRent =>
        researchFundingValues != null && researchFundingValues.Length > 0
            ? researchFundingValues[0]
            : 0;

    public override int[] RentByHouses => researchFundingValues;

    //Needed for IUpgradableTileInfo

    public int UpgradeLevel => GetCurrentUpgradeLevel();

    public int UpgradeCost => dataPointCost;

    public bool CanApplyUpgrade()
    {
        return CanUpgrade();
    }

    public void ApplyUpgrade()
    {
        TryUpgrade();
    }
    
    public bool IsMortgaged => isMortgaged;

}
