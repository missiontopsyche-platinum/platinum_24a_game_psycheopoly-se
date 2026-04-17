using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Events.EventDataStructures;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

/// <summary>
/// Releases the player from jail and resets their jail turn counter.
/// </summary>
[CreateAssetMenu(fileName = "GetOutOfJailCardEffect", menuName = "Card Data/Effects/GetOutOfJailCardEffect")]
public class GetOutOfJailCardEffect : CardEffect
{
    [SerializeField] public NoActionLandingEventChannel noActionLandingEventChannel;
    
    public override void ApplyEffect(Player player)
    {
        // nothing happens on apply effect- this is all handled in CardDeck, Player, and JailUtility
        // we just need to signal that the card has been stored so the turn can continue
        noActionLandingEventChannel?.RaiseEvent(new NoActionLandingEvent(
            "Card added to inventory",
            "Get out of Launchpad Free card has been added to your inventory!"));
    }
}
