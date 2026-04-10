using Logging;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Represents a deck of card ScriptableObjects (Chance or Community Chest).
/// Provides methods for drawing, shuffling, and returning cards to the deck.
/// </summary>
[CreateAssetMenu(fileName = "CardDeck", menuName = "Card Data/Card Deck")]
public class CardDeck : ScriptableObject
{
    public enum DeckType
    {
        Metal,
        Silicate
    };

    [SerializeField] public DeckType deckType;
    [SerializeField] public List<Card> cards = new();
    [SerializeField] public CardDrawnEventChannel cardDrawnChannel;
    public Queue<Card> deckQueue = new();


    public void OnEnable()
    {
        ShuffleDeck();
    }

    public void OnDisable()
    {
        deckQueue.Clear();
    }

    public void ShuffleDeck()
    {
        Shuffle();
    }

    public void DrawCard(Player player)
    {
        if (cards == null || cards.Count == 0)
        {
            Logging.Logger.Error("CardDeck.DrawCard",
                "Deck has no cards configured. Cannot draw.",
                LogCategory.Gameplay,
                this);
            return;
        }

        if (deckQueue == null || deckQueue.Count == 0)
        {
            Logging.Logger.Warn("CardDeck.DrawCard",
                "Deck is empty or null, rebuilding and reshuffling.",
                LogCategory.Gameplay,
                this);

            deckQueue = new Queue<Card>(cards);
            ShuffleDeck();
        }

        Card card = deckQueue.Dequeue();

        if (card == null)
        {
            Logging.Logger.Error("CardDeck.DrawCard",
                "No cards left in the deck to draw.",
                LogCategory.Gameplay,
                this);
            return;
        }

        if (IsGetOutOfJailFreeCard(card))
        {
            // Store both the card and the deck source on the player.
            // This keeps the card out of the deck while held and allows it to
            // be returned to the same deck when used.
            player.AddJailCard(card, this);

            Logging.Logger.Info("CardDeck.DrawCard",
                $"{player.GetPName()} drew a Get Out of Jail Free card from {name}. Card removed from deck until used.",
                LogCategory.Gameplay,
                this);

            return;
        }

        cardDrawnChannel?.Raise(card, player, this);
    }

    public void ReturnCardToDeck(Card card)
    {
        if (card == null)
        {
            Logging.Logger.Error("CardDeck.ReturnCardToDeck",
                "Cannot return a null card to the deck.",
                LogCategory.Gameplay,
                this);
            return;
        }

        // The card is added back into the deck queue only when it is used.
        deckQueue.Enqueue(card);

        Logging.Logger.Info("CardDeck.ReturnCardToDeck",
            $"Returned {card.title} to {name}.",
            LogCategory.Gameplay,
            this);
    }

    private void Shuffle()
    {
        if (cards == null || cards.Count == 0)
            return;

        if (deckQueue == null || deckQueue.Count == 0)
            deckQueue = new Queue<Card>(cards);

        var list = deckQueue.ToList();
        var rng = new System.Random();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        deckQueue = new Queue<Card>(list);
    }


    private bool IsGetOutOfJailFreeCard(Card card)
    {
        return card.effect.Any(effect => effect is GetOutOfJailCardEffect);
    }
}
