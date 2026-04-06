using System;
using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "FreeSpaceData", menuName = "Board Spaces/Gravity Assist Space")]
public class GravityAssistSpaceData : SpaceData
{
    [SerializeField] public String flavorText = "Gravity Assist!";
    [SerializeField] public ActionResolvedEventChannel actionResolvedEventChannel;
    
    public override void OnLanded(Player player)
    {
        // nothing happens
        actionResolvedEventChannel.RaiseEvent(new ActionResolvedEvent(player.GetId()));
    }

    public override void OnPassed(Player player)
    {
        // nothing happens
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();
        
        payload.AppendInformation(flavorText);
        payload.AppendInformation("Free space with no effects.");
        
        spaceHoverEventChannel?.RaiseEvent(payload);

        return payload;
    }
}
