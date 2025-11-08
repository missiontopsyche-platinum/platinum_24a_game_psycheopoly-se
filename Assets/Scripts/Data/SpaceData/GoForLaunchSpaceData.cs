using System;
using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "GoForLaunchSpaceData", menuName = "Board Spaces/Go For Launch Space")]
public class GoForLaunchSpaceData : SpaceData
{
    [SerializeField] public String flavorText = "GO FOR LAUNCH!";
    [SerializeField] public PlayerEventChannel goDirectlyToLaunchPadChannel;
    
    public override void OnLanded(Player player)
    {
        // call event to send player to launch pad (jail)
        goDirectlyToLaunchPadChannel.RaiseEvent(player);
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();

        payload.AppendInformation(flavorText);
        payload.AppendInformation("Go directly to the Launch Pad, do not pass GO.");
        
        return payload;
    }
}