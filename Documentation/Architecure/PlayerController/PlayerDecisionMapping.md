# Player Decision Mapping

**Author(s):** Name(s)
**Date Created:** 00/00/0000
**Last Updated:** 03/18/2026
**Version:** 1.0
s
This document maps game Events to Player decision methods that Controllers must call.

### Format

Each entry follows this pattern:
- **Trigger Event**: The Event that triggers this decision
- **Decision Point**: What the controller needs to decide
- **Player Methods**: The methods the controller will call on the Player

---

### Contents

1. [Property Purchase Decision](#1-property-purchase-decision)
2. [Property Upgrade Decision](#2-property-upgrade-decision)
3. [Rent Payment](#3-rent-payment)
4. [Jail Release Decision](#4-jail-release-decision)
5. [Mortgage/Un-mortgage Decision](#5-mortgageun-mortgage-decision)
6. [Collect GO](#6-collect-go)
7. [End Turn](#7-end-turn)
8. [Asset Liquidation (Bankruptcy Flow)](#8-asset-liquidation-bankruptcy-flow)
9. [Card Effects](#9-card-effects)
10. [Roll Dice](#10-roll-dice)

---

## 1. Property Purchase Decision

- **Trigger Event**: [PurchaseOwnableRequestEvent](../../Assets/Scripts/Events/EventDataStructures/PurchaseOwnableRequestEvent.cs)
  - Carries: `Player requestedPlayer, OwnableSpaceData requestedSpace, int cost`
- **Decision Point**: Should the player purchase the unowned property?
- **Player Methods**:
  - `bool CanAffordProperty(OwnableSpaceData property)` - validation
  - `void ExecutePurchase(OwnableSpaceData property)` - affirmative decision
  - `void DeclinePurchase(OwnableSpaceData property)` - negative decision

---

[_Return to Top_](#player-decision-mapping)

---

## 2. Property Upgrade Decision

- **Trigger Event**: Player-initiated (available during player's turn)
  - Could be: Button press in UI, or AI evaluation during `TurnStartedEvent`
  - No specific event triggers this..?
- **Decision Point**: Should the player upgrade/downgrade any owned properties?
- **Player Methods**:
  - `bool CanUpgradeProperty(PropertySpaceData property)` - validation
  - `bool CanDowngradeProperty(PropertySpaceData property)` - validation
  - `void ExecuteUpgrade(PropertySpaceData property)` - build improvement
  - `void ExecuteDowngrade(PropertySpaceData property)` - sell improvement

**Notes**: This is an *available action* during the player's turn, not a triggered decision.
HumanPlayerController would expose this through UI buttons. AIPlayerController would evaluate
this opportunity during `TurnStartedEvent` or after other actions complete. The UI will need
to query the player's owned properties and filter to upgradable ones.

---

[_Return to Top_](#player-decision-mapping)

---

## 3. Rent Payment

- **Trigger Event**: [ChargeOwnershipFeeEvent](../../Assets/Scripts/Events/EventDataStructures/ChargeOwnershipFeeEvent.cs)
  - Carries: `Player fromPlayer, Player toPlayer, int amount, OwnableSpaceData sourceSpace`
- **Decision Point**: N/A - Automatic payment, no player choice.
- **Player Methods**:
  - `void ExecutePayment(OwnableSpaceData property, Player owner)`

**Notes**: This is triggered by controller, but isn't a decision. Its inclusion here is to 
maintain the pattern of decision flow, but also to allow us to use UI to provide player 
feedback when they land on an owned property in the future.

---

[_Return to Top_](#player-decision-mapping)

---

## 4. Jail Release Decision

- **Trigger Event**: [TurnStartedEvent](../../Assets/Scripts/Events/EventDataStructures/TurnStartedEvent.cs), if Player is in jail
  - Carries: `int playerId, int turnNum`
- **Decision Point**: How should the player attempt to get out of jail?
- **Player Methods**:
  - `bool HasGetOutOfJailCard()` - validation
  - `bool CanAffordJailFee()` - validation
  - `void ExecuteUseGetOutCard()` - use card
  - `void ExecutePayJailFee()` - pay the jail fee to get out and resume turn as normal
  - `void ExecuteRollForRelease()` - try to roll doubles to get of jail

**Notes**: The `TurnStartedEvent` catch method should check for the player in jail, and
then call the decision method in Controller if they are, rather than having separate
subscriptions to `TurnStartedEvent`, since we will also use that to determine if a
controller should be responding to events at all.

---

[_Return to Top_](#player-decision-mapping)

---

## 5. Mortgage/Un-mortgage Decision

- **Trigger Event**: Player-initiated (available during player's turn) OR forced by insufficient funds
- **Decision Point**: Player decides which properties to mortgage/un-mortgage
- **Player Methods**:
  - `bool CanMortgageProperty(OwnableSpaceData property)` - validation
  - `bool CanUnmortgageProperty(OwnableSpaceData property)` - validation
  - `void ExecuteMortgage(OwnableSpaceData property)` - mortgage the property
  - `void ExecuteUnmortgage(OwnableSpaceData property)` - un-mortgage the property (pay it off)

**Notes**: Usually player-initiated, but may become mandatory if player needs to raise
funds to pay rent/fees. This would be part of asset liquidation. This would be another
UI window where a player can select properties to mortgage or un-mortgage, depending on
the scenario, similar to the upgrade UI.

---

[_Return to Top_](#player-decision-mapping)

---

## 6. Collect GO

- **Trigger Event**: `PlayerCollectsGoEvent` (Does not exist, current flow just pays player directly)
  - Carries: `Player player, int amount`
- **Decision Point**: N/A - Automatic collection
- **Player Methods**:
  - `void ExecuteCollectGo(int amount)`

**Notes**: No decision needed. Controller receives event and immediately calls Player method.
May want UI feedback to show player receiving money for passing go.

---

[_Return to Top_](#player-decision-mapping)

---

## 7. End Turn

- **Trigger Event**: Player-initiated (button press) OR automatic after certain actions
  - Could be: Button press in UI, or automatic after dice roll when no other actions available
- **Decision Point**: Is the player ready to end their turn?
- **Player Methods**:
  - `bool CanEndTurn()` - validation (has rolled dice, completed mandatory actions)
  - `void ExecuteEndTurn()` - ends turn, passes control to next player

**Notes**: HumanPlayerController exposes "End Turn" button. AIPlayerController calls
this after completing its decision logic. GameManager validates turn completion requirements.

---

[_Return to Top_](#player-decision-mapping)

---

## 8. Asset Liquidation (Bankruptcy Flow)

- **Trigger Event**: No specific event. Triggered when Player methods like `ExecutePayment()`
  fail validation (insufficient funds).
- **Decision Point**: Which assets should the player sell/mortgage to raise funds?
- **Player Methods**:
  - `List<OwnableSpaceData> GetLiquidatableAssets()` - query available assets
  - `int GetLiquidationValue(OwnableSpaceData property)` - calculate potential value. This could be added to `OwnableSpaceData`
  - `void ExecuteAssetLiquidation(List<OwnableSpaceData> assetsToLiquidate)` - execute sales
  - `bool CanSatisfyDebt(int amountOwed)` - validation after liquidation

**Notes**: This is triggered when player can't afford rent/fees. Controller must help
player select assets to liquidate. If player still can't pay after liquidating everything,
they are bankrupt and eliminated. This is complex and may be partially deferred, but the
entry point should be documented.

---

[_Return to Top_](#player-decision-mapping)

---

## 9. Card Effects

- **Trigger Event**: `CardDrawnEvent` (needs to be refactored from current brute-force approach)
  - Carries: `Player player, CardData card`
- **Decision Point**: Card effects happen automatically.
- **Player Methods**:
  - `void ExecuteCardEffect(CardData card)` - apply card effect
  - `void AddCardToInventory(CardData card)` - for keepable cards

**Notes**: Most cards are automatic effects. Some (like GOOJ card) are stored for later use.
We're moving the flow to `PlayerController` to unify player feedback and UI controls.

---

[_Return to Top_](#player-decision-mapping)

---

## 10. Roll Dice

**Trigger**: Player-initiated during their turn (when they haven't rolled yet)
- Available at turn start
- HumanPlayerController exposes "Roll Dice" button
- AIPlayerController automatically triggers after processing turn start

**Decision Point**: Is the player ready to roll the dice and move?

**Player Methods**:
- `bool CanRollDice()` - validation (hasn't rolled this turn, not in jail attempting roll, etc.)
- `void ExecuteRollDice()` - triggers dice roll, publishes result event

**Notes**: Previously automatic at turn start. Moving to player-initiated for consistency
with controller pattern. HumanPlayerController shows "Roll Dice" button at turn start.
AIPlayerController calls this automatically after minimal delay (for realism/visibility).
GameManager handles actual dice roll logic and movement after Player publishes roll event.
Taking this out of turn phase order for human player aligns with Property Management being
out of turn phase, and gives more autonomy to human players.

---

[_Return to Top_](#player-decision-mapping)

---