using Events.EventDataStructures;
using UnityEngine;

public class GoSpaceData : SpaceData
{
    [SerializeField] public int payout = 200;
    [SerializeField] public PayPlayerEventChannel payPlayerEventChannel;
    
    public override void OnLanded(Player player)
    {
        RaisePayEvent(player);
    }

    public override void OnPassed(Player player)
    {
        RaisePayEvent(player);
    }

    private void RaisePayEvent(Player player)
    {
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(
            player, payout));
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();
        
        payload.AppendInformation($"On passing or landing, collect ${payout}.");
        spaceHoverEventChannel?.RaiseEvent(payload);

        return payload;
    }
}