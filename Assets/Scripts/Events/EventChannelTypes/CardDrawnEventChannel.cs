using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CardDrawnEventChannel",
    menuName = "Events/Card Drawn Event Channel")]
public class CardDrawnEventChannel : ScriptableObject
{
    public event Action<Card, Player, CardDeck> OnCardDrawn;

    public void Raise(Card card, Player player, CardDeck deck)
    {
        OnCardDrawn?.Invoke(card, player, deck);
    }

    public void Subscribe(Action<Card, Player, CardDeck> listener)
    {
        OnCardDrawn += listener;
    }

    public void Unsubscribe(Action<Card, Player, CardDeck> listener)
    {
        OnCardDrawn -= listener;
    }
}
