using System;
using System.Collections.Generic;
using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "LaunchPadSpaceData", menuName = "Board Spaces/Launch Pad Space")]
public class LaunchPadSpaceData : SpaceData
{
    [SerializeField] public String flavorText = "WAITING FOR LAUNCH.";
    [SerializeField] public PlayerEventChannel playerGoesToJailEventChannel;
    [SerializeField] public PlayerEventChannel playerLeavesJailEventChannel;

    private List<Player> playersInJail = new ();
    
    public override void OnLanded(Player player)
    {
        // do nothing
        
        // right now, this whole thing is basically a data container to be used by the UI to
        // tell a user who is in jail right now... Maybe we add some other logic, but for 
        // now this seems satisfactory.
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();
        
        payload.AppendInformation(flavorText);
        payload.AppendInformation("Players Waiting:");
        foreach (Player player in playersInJail)
            payload.AppendInformation(player.GetPName());
        
        spaceHoverEventChannel?.RaiseEvent(payload);

        return payload;
    }

    private void AddPlayerToJail(Player player) => playersInJail.Add(player);
    private void RemovePlayerFromJail(Player player) => playersInJail.Remove(player);

    private void OnEnable()
    {
        playerGoesToJailEventChannel?.Subscribe(AddPlayerToJail);
        playerLeavesJailEventChannel?.Subscribe(RemovePlayerFromJail);
    }
    
    private void OnDisable()
    {
        playerGoesToJailEventChannel?.Unsubscribe(AddPlayerToJail);
        playerLeavesJailEventChannel?.Unsubscribe(RemovePlayerFromJail);
    }
}
