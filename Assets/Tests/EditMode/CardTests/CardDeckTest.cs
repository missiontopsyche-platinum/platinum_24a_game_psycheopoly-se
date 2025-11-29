using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Tests.EditMode;
using UnityEngine;

public class CardDeckTest : ManagerTestBase
{
    private CardDeck deck;
    private Player player;

    [SetUp]
    public void SetUp()
    {
        InitializeTestLogger();

        deck = ScriptableObject.CreateInstance<CardDeck>();

        player = ScriptableObject.CreateInstance<Player>();
        player.SetId(0);
        player.SetPName("Tester");
        player.SetMoney(1500);
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestObjects(player, deck);
    }

    private Card MakeCard(params CardEffect[] effects)
    {
        var card = ScriptableObject.CreateInstance<Card>();
        card.effect = new List<CardEffect>(effects);
        return card;
    }

    private void SeedDeckCards(IEnumerable<Card> cardInstances)
    {
        List<Card> cards = null;

        if (deck.cards == null)
        {
            cards = new List<Card>();
            deck.cards = cards;
        }
        else
            cards = deck.cards;

        foreach (var cardInstance in cardInstances)
        {
            cards.Add(cardInstance);
        }

        deck.OnEnable();
    }

    private bool HasAGetOutOfJailCard(List<Card> list)
    {
        return list.Any(c => c.effect.Any(e => e is GetOutOfJailCardEffect));
    }

    [Test]
    public void ShuffleDeck_PreservesCardCount()
    {
        // Two cards with no effects, order matters
        var c1 = (Card)MakeCard();
        var c2 = (Card)MakeCard();
        SeedDeckCards(new Card[] { c1, c2 });

        // Calling Shuffle should not change the count or crash
        Assert.DoesNotThrow(() => deck.ShuffleDeck());
        Assert.AreEqual(2, deck.cards.Count);

        // Draw twice
        Assert.DoesNotThrow(() => deck.DrawCard(player));
        Assert.DoesNotThrow(() => deck.DrawCard(player));
        Assert.AreEqual(2, deck.cards.Count);
    }

    [Test]
    public void DrawCard_EnsureCardIsDrawn()
    {
        var cardDrawnChannel = CreateChannel<CardDrawnEventChannel>();
        var raisedMoves = new List<Card>();
        cardDrawnChannel.Subscribe((card, player, deck)=> { raisedMoves.Add(card); });
        deck.cardDrawnChannel = cardDrawnChannel;
        // Card A
        var a1 = new MoveCardEffect { Type = MoveCardEffect.EffectType.MoveForward, SpacesToMove = 2 };
        var a2 = new MoveCardEffect { Type = MoveCardEffect.EffectType.MoveBackward, SpacesToMove = 1 };
        var cardA = MakeCard(a1, a2);

        // Card B
        var b1 = new MoveCardEffect { Type = MoveCardEffect.EffectType.MoveForward, SpacesToMove = 3 };
        var b2 = new MoveCardEffect { Type = MoveCardEffect.EffectType.MoveBackward, SpacesToMove = 2 };
        var cardB = MakeCard(b1, b2);

        // Seed and let OnEnable build & shuffle the deckqueue
        SeedDeckCards(new Card[] { cardA, cardB });

        // First draw produces 1 draw events
        raisedMoves.Clear();
        deck.DrawCard(player);
        Assert.AreEqual(1, raisedMoves.Count);
        var firstPair = raisedMoves
            .SelectMany(c => c.effect)                
            .OfType<MoveCardEffect>()           
            .Select(e => e.SpacesToMove)            
            .ToArray();
        deck.ReturnCardToDeck(raisedMoves[0]);

        bool isA = firstPair.SequenceEqual(new[] { 2, 1 });
        bool isB = firstPair.SequenceEqual(new[] { 3, 2 });
        Assert.IsTrue(isA || isB, "First draw should be either Card A (+2,-1) or Card B (+3,-2).");

        raisedMoves.Clear();
        deck.DrawCard(player);
        Assert.AreEqual(1, raisedMoves.Count);
        var secondPair = raisedMoves
            .SelectMany(c => c.effect)               
            .OfType<MoveCardEffect>()           
            .Select(e => e.SpacesToMove)             
            .ToArray();
        deck.ReturnCardToDeck(raisedMoves[0]);

        if (isA)
            CollectionAssert.AreEqual(new[] { 3, 2 }, secondPair);
        else
            CollectionAssert.AreEqual(new[] { 2, 1 }, secondPair);

        // Since cards are returned to the bottom, we test it again.
        raisedMoves.Clear();
        deck.DrawCard(player);
        Assert.AreEqual(1, raisedMoves.Count);
        var third = raisedMoves
            .SelectMany(c => c.effect)             
            .OfType<MoveCardEffect>()      
            .Select(e => e.SpacesToMove) 
            .ToArray();

        Assert.IsTrue(third.SequenceEqual(new[] { 2, 1 }) || third.SequenceEqual(new[] { 3, 2 }));
    }

    [Test]
    public void GetOutOfJailFreeCard_EnsureItDoesNotReturnAfterDrawn()
    {
        var a1 = new MoveCardEffect { Type = MoveCardEffect.EffectType.MoveForward, SpacesToMove = 2, MovePlayerEventChannel = null };
        var a2 = new GetOutOfJailCardEffect();
        var cardA = MakeCard(a1, a2);

        var b1 = new MoveCardEffect { Type = MoveCardEffect.EffectType.MoveForward, SpacesToMove = 3, MovePlayerEventChannel = null };
        var b2 = new GetOutOfJailCardEffect();
        var cardB = MakeCard(b1, b2);

        SeedDeckCards(new[] { cardA, cardB });
        List<Card> cards = player.GetGetOutOfJailCards();
        // Make sure the player doesnt already have a get out of jail free card.
        Assert.False(HasAGetOutOfJailCard(cards));
        // Jail cards should not execute, giving the player the option to store them.
        deck.DrawCard(player);
        Assert.AreEqual(deck.deckQueue.Count, 1);
        cards = player.GetGetOutOfJailCards();
        Assert.True(HasAGetOutOfJailCard(cards));
    }
}
