using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "PropertySpaceData", menuName = "Board Spaces/Property Space")]
public class PropertySpaceData : OwnableSpaceData
{
    [Header("Property-Specific Values")]
    [SerializeField] public int[] researchFundingValues = new int[6];
    [SerializeField] public int dataPointCost;

    [Header("Property Event Channels")] 
    [SerializeField] private PurchaseUpgradeRequestEventChannel purchaseUpgradeRequestEventChannel;
    
    private int currentUpgradeLevel = 0;

    public override void OnLanded(Player player)
    {
        base.OnLanded(player);
        if (owner == null) return;
        
        if (owner.Equals(player))
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
        
        // fire event payload
        spaceHoverEventChannel?.RaiseEvent(payload);
        return payload;
    }

    /// <summary>
    /// Called by GameManager if Upgrade Request is approved, increments Upgrade Level by 1.
    /// </summary>
    public void UpgradeProperty()
    {
        currentUpgradeLevel += 1;
    }
}
