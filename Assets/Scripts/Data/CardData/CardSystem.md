# Card System Documentation

## Overview 
Each **Card** contains a sequence of **CardEffects**, and each effect publishes events that other systems (e.g. BoardManager, PlayerManager, UI) respond to.  
Effects do *not* modify game state directly.

This system offers:

- Modular effects that can be reused between cards  
- Support for multi-effect cards  
- Polymorphic effect lists using `SerializeReference`  
- Clean event-driven design  
- Easy testing and extension  

---

# System Architecture

```
CardDeck
   ↓ draws a Card
Card
   ↓ contains a list of
CardEffects
   ↓ publish events to
Event Channels
   ↓ consumed by
BoardManager / PlayerManager / JailSystem
```

---

# 1. Card

### Description

A Card is a ScriptableObject that contains text and an ordered list of CardEffects.

### XML Summary
Represents a single Chance or Community Chest style card.
Each card contains UI text and an ordered list of CardEffects
that execute sequentially when the card is drawn.

Cards store their effects using [SerializeReference], allowing
composable and polymorphic effect objects without creating
new ScriptableObjects for every card.

### Runtime Flow

```
Draw Card
→ Display UI
→ For each effect: ApplyEffect(player)
```

---

# 2. CardEffect (Base Class)

### Description

Base class for all card actions.  
Every effect triggers exactly one gameplay behavior: move, pay, charge, jail update, etc.

### XML Summary

Abstract base class for all card effects.
A CardEffect represents a single unit of gameplay behavior
(movement, payments, jail interactions, etc.) that can be combined
with other effects to form a complete card.

CardEffects are ScriptableObjects so they can be reused,
configured independently, and assigned to multiple cards.

Each effect implements ApplyEffect and typically publishes
events rather than modifying game state directly.

---

# 3. Concrete Card Effects

Below is documentation for each effect type in the system.

---

## 3.1 MoveCardEffect

### Summary

Moves the player forward or backward a fixed number of spaces.

### XML

Moves a player forward or backward a fixed number of spaces.
Publishes a MovePlayerEvent through MovePlayerEventChannel.
BoardManager receives the event and updates player position.

---

## 3.2 MoveToSpaceCardEffect

### Summary

Requests teleportation to a specific *type* of space (GO, nearest property, nearest card).

### XML

Requests that a player be moved to a specific type of board space,
such as GO, the nearest property, or the nearest card-drawing space.

This effect raises a MoveToSpaceEvent, and a rule subsystem
resolves the actual board index.

---

## 3.3 GoToJailCardEffect

### Summary

Sends the player to jail and sets jail turn count.

### XML

Sends the player directly to jail for a configured number of turns.
Raises a JailStateChangedEvent with InJail = true.
Movement to the jail tile is handled elsewhere.

---

## 3.4 GetOutOfJailCardEffect

### Summary

Releases the player from jail.

### XML


Releases the player from jail and resets their jail turn counter.
Does not manage possession of the "Get Out of Jail Free" card—
that is tracked on the Player object.


---

## 3.5 CollectFromAllPlayersCardEffect

### Summary

All other players pay the acting player a fixed amount.

### XML

Causes all other players to pay a fixed amount to the acting player.Publishes a MoneyDistributionEvent processed by PlayerManager.

---

## 3.6 PayAllPlayersCardEffect

### Summary

Acting player pays every other player a fixed amount.

### XML

Causes the acting player to pay every other player a fixed amount.
Publishes a MoneyDistributionEvent, which PlayerManager uses
to distribute funds to opponents.

---

## 3.7 CollectPerPropertyCardEffect

### Summary

Pays money to the player based on owned houses and hotels.

### XML

Pays the acting player based on the number of houses and hotels owned.

Upgrade levels:
- 1–4 = houses
- 5   = hotel

Total:
(ChargeForHouse × houses) + (ChargeForHotel × hotels)
Publishes a PayPlayerEvent to deposit funds.

---

## 3.8 PayPerPropertyCardEffect

### Summary

Charges the player based on owned houses and hotels.

### XML

Charges the acting player based on the number of houses and hotels owned.

Upgrade levels:
- 1–4 = houses
- 5   = hotel

Total:
(ChargeForHouse × houses) + (ChargeForHotel × hotels)

Publishes a ChargePlayerEvent to withdraw funds.

---

# 4. CardDeck

### Summary

Deck managing draw, shuffle, and card recycling.

### XML Documentation

Represents a deck of card ScriptableObjects (Chance or Community Chest).
Provides methods for drawing, shuffling, and returning cards to the deck.
Decks do not execute card logic; callers must execute effects manually.

### Typical Flow

```
card = deck.Draw()
→ Show card UI
→ For each effect in card:
       effect.ApplyEffect(player)
→ deck.ReturnToBottom(card)
```

---

# 5. Integrations With Other Systems

### Movement
- Effects publish events to MovePlayerEventChannel / MoveToSpaceEventChannel.
- BoardManager resolves actual movement.

### Money
- Payment and collection effects raise events consumed by PlayerManager.

### Jail
- Jail events modify Player jail state via PlayerManager + jail strategy.

---

# 6. Creating a New Card

1. Create a **Card** asset.
2. Add one or more **CardEffect** assets.
3. Set card title + description.
4. Add the card to a **CardDeck**.

Effects execute **top to bottom**.

---

# 7. Creating Custom CardEffects

To add new mechanics:

```csharp
[CreateAssetMenu(menuName = "Card Data/Effects/MyCustomEffect")]
public class MyCustomEffect : CardEffect
{
    [SerializeField] int value;
    [SerializeField] SomeEventChannel channel;

    /// <summary>
    /// Describe the behavior of your custom effect.
    /// </summary>
    public override void ApplyEffect(Player player)
    {
        channel.RaiseEvent(new SomeEvent(player, value));
    }
}
```

Add it to any card through the Inspector.

---

# 8. Example Multi-Effect Card

### “Advance 3 spaces and pay each player $25.”

Effects (in order):

1. MoveCardEffect — SpacesToMove = 3  
2. PayAllPlayersCardEffect — Amount = 25  

Execution order is guaranteed by list order.

---

# 9. Summary

The Card System is:

- Event-driven  
- Polymorphic  
- Modular  
- Extensible  
- Designer-friendly  
- Easy to test  

Perfect for Monopoly-like card mechanics.

