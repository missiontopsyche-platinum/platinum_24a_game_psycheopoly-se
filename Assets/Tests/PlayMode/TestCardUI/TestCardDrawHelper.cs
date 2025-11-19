using UnityEngine;

/// <summary>
/// Simple helper used only by PlayMode tests to trigger card draws
/// </summary>
public class TestCardDrawHelper : MonoBehaviour
{
    [Tooltip("Deck to draw cards from in tests.")]
    public CardDeck deck;

    [Tooltip("Player ScriptableObject used for test draws.")]
    public Player testPlayer;

    /// <summary>
    /// Called by PlayMode tests to draw a single card
    /// </summary>
    public void DrawOnce()
    {
        if (deck == null || testPlayer == null)
        {
            Debug.LogError("TestCardDrawHelper: deck or testPlayer is not assigned.");
            return;
        }

        deck.DrawCard(testPlayer);
    }
}
