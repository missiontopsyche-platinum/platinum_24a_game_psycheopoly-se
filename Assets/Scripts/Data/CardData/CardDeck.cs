using Logging;
using System.Collections.Generic;
using System.Linq;
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
    public Queue<Card> deckQueue;
    [SerializeField] private CardDrawnEventChannel cardDrawnChannel;

    private Queue<Card> deckQueue = new();

    public void OnEnable()
    {
        deckQueue = new Queue<Card>(cards);
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
            // TODO: When refactoring, call event channel to notify all components
            player.AddJailCard(card);
            return;
        }

        // The execution happens within the deck.
        // This is how I understand from the code structure we have so far.
        foreach (var effect in card.effect)
        {
            effect.ApplyEffect(player);
        }
        ReturnCardToDeck(card);
    }

    public void ReturnCardToDeck(Card card)
    {
        // TODO: When refactoring, call event channel to notify all components
        deckQueue.Enqueue(card);
    }

    private void Shuffle()
    {
        if (cards.Count == 0) return;
        if (deckQueue == null) deckQueue = new Queue<Card>(cards);


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
