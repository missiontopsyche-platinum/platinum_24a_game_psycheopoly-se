using Events.EventDataStructures;
using UnityEngine;
using Assets.Scripts.Managers.Rent;

public abstract class OwnableSpaceData : SpaceData, ITileRentInfo
{
    [SerializeField] public int buyPrice; // board value, price for first purchase
    [SerializeField] public int collaborationValue; // AKA Mortgage value
    protected Player owner;

    // used for mortage information
    public bool isMortgaged = false;
    public bool isMortgageable { get; set; } = true;


    public int mortgagePayoffValue { get; set;  }

    public string Name => name;
    public abstract TileType Type { get; }
    public abstract ColorGroup Group { get; }
    public abstract int HouseCount { get; }
    public abstract int BaseRent { get; }
    public abstract int[] RentByHouses { get; }

    bool ITileRentInfo.IsMortgaged => isMortgaged;

    [Header("Ownable Event Channels")]
    [SerializeField] public PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
    [SerializeField] public ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;
    [SerializeField] public NoActionLandingEventChannel noLandingActionEventChannel;

    public void ResetData()
    {
        owner = null;
        isMortgaged = false;
        isMortgageable = true;
    }

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
        payload.AppendInformation($"Owned By: {(owner != null ? owner.GetPName() : "Unowned")}");
        payload.AppendInformation($"Ownership Status: {(owner == null ? "Unowned" : "Owned")}");
        //TODO: integrate upgrade level visibility logic
        payload.AppendInformation($"Upgrade Level: [Upgrade Level]");
        
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

    public bool GetIsMortageable()
    {

        return this.isMortgageable;
    }
}
