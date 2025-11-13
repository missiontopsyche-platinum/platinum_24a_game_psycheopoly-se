using UnityEngine;

[CreateAssetMenu(fileName = "CardDeck", menuName = "Card Data/Card Deck")]
public class CardDeck : ScriptableObject
{
    public enum DeckType
    {
        Metal,
        Silicate
    };

    [SerializeField] public DeckType deckType;
    
    // Placeholder. Will contain a list of Cards, and methods to draw cards and apply effects.

    public void DrawCard(Player player) {}
}
