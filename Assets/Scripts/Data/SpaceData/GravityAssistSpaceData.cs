using System;
using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "FreeSpaceData", menuName = "Board Spaces/Gravity Assist Space")]
public class GravityAssistSpaceData : SpaceData
{
    [SerializeField] public String flavorText = "Gravity Assist!";
    
    public override void OnLanded(Player player)
    {
        // nothing happens
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

        return payload;
    }
}
