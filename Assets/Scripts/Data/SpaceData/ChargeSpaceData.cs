using System;
using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "ChargeSpaceData", menuName = "Board Spaces/Charge Space")]
public class ChargeSpaceData : SpaceData
{
    [SerializeField] private int cost = 200;
    [SerializeField] private String flavorText = "Buy new beakers.";
    [SerializeField] private ChargePlayerEventChannel chargePlayerEventChannel;
    
    public override void OnLanded(Player player)
    {
        // charge the player the amount determined by the space.
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(
            player, cost));
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();

        payload.AppendInformation(flavorText);
        payload.AppendInformation($"Pay {cost}.");
        
        return payload;
    }
}
