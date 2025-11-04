using Events.EventDataStructures;
using UnityEngine;

public abstract class OwnableSpaceData : SpaceData
{ 
    [SerializeField] public int collaborationValue;
    protected Player owner;

    [Header("Ownable Event Channels")]
    [SerializeField] protected PurchaseOwnableRequestEventChannel purchaseOwnableRequestEventChannel;
    [SerializeField] protected ChargeOwnershipFeeEventChannel chargeOwnershipFeeEventChannel;

    public override SpaceHoverEvent OnHover()
    {
        var payload = base.OnHover();
        payload.AppendInformation($"Collaboration Value: ${collaborationValue}");
        payload.AppendInformation($"Owner: {(owner ? owner.GetPName() : "None")}");
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
}
