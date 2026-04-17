using System;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "GoForLaunchSpaceData", menuName = "Board Spaces/Go For Launch Space")]
public class GoForLaunchSpaceData : SpaceData
{
    [SerializeField] public String flavorText = "GO FOR LAUNCH!";
    [SerializeField] public PlayerEventChannel goDirectlyToLaunchPadChannel;
    [SerializeField] public JailStateChangedEventChannel jailStateChangedEventChannel;
    
    public override void OnLanded(Player player)
    {
        // call event to send player to launch pad (jail)
        goDirectlyToLaunchPadChannel?.RaiseEvent(player);
        
        // this is the actual jail game event. We can still use the above for UI, but it seems like we can just remove that one.
        jailStateChangedEventChannel?.RaiseEvent(new JailStateChangedEvent(player, true, 0));
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
        
        spaceHoverEventChannel?.RaiseEvent(payload);
        
        return payload;
    }
}