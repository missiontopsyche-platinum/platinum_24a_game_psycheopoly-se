using Events.EventDataStructures;
using UnityEngine;

public abstract class OwnableSpaceData : SpaceData
{
    [SerializeField] public int buyPrice; // board value, price for first purchase
    [SerializeField] public int collaborationValue; // AKA Mortgage value
    protected Player owner;
    public bool isMortaged { get; set; }

    [Header("Ownable Event Channels")]
    [SerializeField] public PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
    [SerializeField] public ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;

    public override void OnLanded(Player player)
    {
        // moved into OwnableSpaceData because every ownable should offer to buy if no owner
        if (owner == null)
        {
            // offer to buy UI in the future
            purchaseOwnableRequestEventChannel?.RaiseEvent(new PurchaseOwnableRequestEvent(
                player,
                this,
                buyPrice));
        }
        // because theres no return type, its not possible currently for an inherited class to know
        // if the owner was null this turn, and we just have to hope that owner stays null through the
        // rest of execution for this OnLanded().
        
        // we can refactor in the future to add a return value boolean or something to mitigate that
        // risk.
    }

    public override SpaceHoverEvent OnHover()
    {
        var payload = base.OnHover();
        payload.AppendInformation($"Purchase Price: {buyPrice}");
        payload.AppendInformation($"Collaboration Value: ${collaborationValue}");
        payload.AppendInformation($"Owner: {(owner ? owner.GetPName() : "None")}");
        
        spaceHoverEventChannel?.RaiseEvent(payload);
        
        return payload;
    }

    /// <summary>
    /// Sets the owner of this property. Called by GameManager when a purchase is completed.
    /// </summary>
    /// <param name="newOwner">Player that is the new owner.</param>
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
    }

    public Player GetOwner() => owner;
}
