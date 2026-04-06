using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// Releases the player from jail and resets their jail turn counter.
/// </summary>
[CreateAssetMenu(fileName = "GetOutOfJailCardEffect", menuName = "Card Data/Effects/GetOutOfJailCardEffect")]
public class GetOutOfJailCardEffect : CardEffect
{
    [SerializeField] public ActionResolvedEventChannel actionResolvedEventChannel;
    
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;

        // TODO: Add this card to player inventory, remove from card deck.
        
        actionResolvedEventChannel.RaiseEvent(new ActionResolvedEvent(player.GetId()));
    }
}
