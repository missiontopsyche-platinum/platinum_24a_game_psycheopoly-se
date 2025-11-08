using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSpaceData", menuName = "Board Spaces/Card Space")]
public class CardSpaceData : SpaceData
{
    [SerializeField] public CardDeck cardDeck;
    
    public override void OnLanded(Player player)
    {
        // TODO: Flesh out implementation based on how CardDeck and Cards are implemented.
        // It seems like keeping everything separate makes sense tho, the Space doesnt need to
        // have any logical impact on what a card does.
        cardDeck.DrawCard(player);
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();

        switch (cardDeck.deckType)
        {
            case CardDeck.DeckType.Metal:
                payload.AppendInformation("Draw a card from the Metal deck.");
                break;
            case CardDeck.DeckType.Silicate:
                payload.AppendInformation("Draw a card from the Silicate deck.");
                break;
        }
        
        return payload;
    }
}
