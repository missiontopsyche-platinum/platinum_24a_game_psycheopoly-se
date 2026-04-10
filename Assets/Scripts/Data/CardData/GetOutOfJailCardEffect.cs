using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

/// <summary>
/// Releases the player from jail and resets their jail turn counter.
/// </summary>
[CreateAssetMenu(fileName = "GetOutOfJailCardEffect", menuName = "Card Data/Effects/GetOutOfJailCardEffect")]
public class GetOutOfJailCardEffect : CardEffect
{
    [SerializeField] public JailStateChangedEventChannel JailStateChangedEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;

        if (JailStateChangedEventChannel == null)
        {
            Logging.Logger.Error("GetOutOfJailCardEffect.ApplyEffect",
                "JailStateChangedEventChannel is not assigned in GetOutOfJailCardEffect.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        // A Get Out of Jail Free card is used from the player's
        // inventory and returned to its source deck.
        if (!player.TryConsumeGetOutOfJailFreeCard(out Card usedCard, out CardDeck sourceDeck))
        {
            Logging.Logger.Warn("GetOutOfJailCardEffect.ApplyEffect",
                $"{player.GetPName()} tried to use a Get Out of Jail Free card but none was available in inventory.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        // Return the used card to the same deck it originally came from.
        sourceDeck.ReturnCardToDeck(usedCard);

        JailStateChangedEventChannel.RaiseEvent(new JailStateChangedEvent(player, false, 0));
    }
}
