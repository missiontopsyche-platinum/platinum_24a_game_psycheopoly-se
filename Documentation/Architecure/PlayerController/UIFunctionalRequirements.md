# UI Functional Requirements

**Author(s):** Name(s)
**Date Created:** 00/00/0000
**Last Updated:** 03/18/2026
**Version:** 1.0


This document describes the functional requirements for UI elements needed for
player decisions.

### Format

Each UI description includes:

- **Purpose:** What decision this UI supports
- **Trigger**: What causes this UI to appear
- **Information Display**: What data must be shown to the player
- **Interactive Elements**: Buttons/controls and their enabled/disabled states
- **Actions**: What happens when each element is interacted with
- **Dismissal**: How/when the UI closes

---

### Contents

1. [Property Purchase UI](#1-property-purchase-ui)
2. [Jail Release UI](#2-jail-release-ui)
3. [Rent Payment Notification UI](#3-rent-payment-notification-ui)
4. [Property Management UI (Unified)](#4-property-management-ui-unified)
5. [Dice Roll UI](#5-dice-roll-ui)

---

## 1. Property Purchase UI

**Purpose**: Allow player to decide whether to purchase an unowned property

**Trigger**: `PurchaseOwnableRequestEvent` received by HumanPlayerController

**Information Display**:
- Property name
- Property cost
- Property color group (for context)
- Property image?
- Player's current money
- Affordability message (if can't afford)

**Interactive Elements**:
- **Buy Button**
    - Enabled when: `Player.CanAffordProperty() == true`
    - Disabled when: `Player.CanAffordProperty() == false`
    - On click: Calls `Player.ExecutePurchase(property)`, shows success feedback
- **Decline Button**
    - Always enabled
    - On click: Calls `Player.DeclinePurchase(property)`, shows decline feedback

**Dismissal**: Automatically closes after Buy or Decline is selected. Blocking, player 
must make a choice.

---

[_Return to Top_](#ui-functional-requirements)

---

## 2. Jail Release UI

**Purpose**: Allow player to choose how to attempt jail release at turn start

**Trigger**: `TurnStartedEvent` when player is in jail, handled by HumanPlayerController

**Information Display**:
- "You are in jail" message
- Available options and their requirements:
    - "Use Get Out of Jail Free Card" (if player has one)
    - "Pay $XX fine" (if player can afford)
    - "Roll for doubles" (always available)
- Player's current money
- Number of turns in jail (for context)

**Interactive Elements**:
- **Use Card Button**:
    - Enabled when: `Player.HasGetOutOfJailCard() == true`
    - Disabled when: Player doesn't have card
    - On click: Calls `Player.ExecuteUseGetOutCard()`, shows result
- **Pay Fine Button**:
    - Enabled when: `Player.CanAffordJailFee() == true`
    - Disabled when: Player can't afford $50
    - On click: Calls `Player.ExecutePayJailFee()`, shows result
- **Roll Dice Button**:
    - Always enabled
    - On click: Calls `Player.ExecuteRollForRelease()`, shows dice result and outcome

**Dismissal**: Automatically closes after player selects an option and the result is processed. Modal/blocking (must choose release method before turn continues).

---

[_Return to Top_](#ui-functional-requirements)

---

## 3. Rent Payment Notification UI

**Purpose**: Inform player they landed on owned property and must pay rent

**Trigger**: `ChargeOwnershipFeeEvent` received by HumanPlayerController

**Information Display**:
- "You landed on [Property Name]" message
- Property owner's name
- Rent amount owed
- Player's money before payment
- Insufficient funds warning (if applicable)

**Interactive Elements**:
- **Acknowledge Button**:
    - Always enabled
    - On click: Dismisses notification after `Player.ExecutePayment()` completes

**Dismissal**: Player clicks Acknowledge, or auto-dismisses after 3-5 seconds (informational only, not blocking).

**Notes**: This is not strictly a "decision" UI since payment is automatic, but provides important feedback to the player. Lower priority than decision UIs.

---

[_Return to Top_](#ui-functional-requirements)

---

## 4. Property Management UI (Unified)

**Purpose**: Manage all property-related actions (upgrades, mortgages, and forced liquidation)

**Modes**:
- **Standard Mode**: Voluntary management during player's turn
- **Liquidation Mode**: Forced management when player needs to raise funds

**Trigger**:
- Standard Mode: Player clicks "Manage Properties" button during their turn
- Liquidation Mode: Automatically triggered when player has insufficient funds to pay debt

**Information Display**:
- Player's current money
- **Liquidation Mode Only**:
    - Amount owed
    - Amount still needed to raise
    - "You must raise funds to continue" message
- List of owned properties, for each showing:
    - Property name and color group
    - Current improvements (0-5: none, 1-4 houses, hotel)
    - Mortgage status (mortgaged/unmortgaged)
    - **Available Actions** (context-dependent):
        - **Add Improvement**: Cost to upgrade (if eligible)
        - **Remove Improvement**: Refund value (if eligible)
        - **Mortgage**: Amount received (if eligible)
        - **Unmortgage**: Cost to pay (if eligible)
    - Eligibility indicators (why actions are disabled if not available)

**Interactive Elements**:
- **Property List/Selection**: Click property to view available actions
  - **Action Buttons** (per property, shown based on context):
      - **Add Improvement (+)**:
          - Enabled when: `Player.CanUpgradeProperty(property) == true`
          - Disabled when: Can't afford, no color group, max improvements, or in Liquidation Mode
          - On click: Calls `Player.ExecuteUpgrade(property)`, updates display
      - **Remove Improvement (-)**:
          - Enabled when: `Player.CanDowngradeProperty(property) == true`
          - Disabled when: No improvements to remove
          - On click: Calls `Player.ExecuteDowngrade(property)`, updates display
      - **Mortgage**:
          - Enabled when: `Player.CanMortgageProperty(property) == true`
          - Disabled when: Already mortgaged or has improvements
          - On click: Calls `Player.ExecuteMortgage(property)`, updates display
      - **Unmortgage**:
          - Enabled when: `Player.CanUnmortgageProperty(property) == true`
          - Disabled when: Not mortgaged, can't afford unmortgage cost, or in Liquidation Mode
          - On click: Calls `Player.ExecuteUnmortgage(property)`, updates display
- **Close/Done Button**:
    - Standard Mode: Always enabled
    - Liquidation Mode: Enabled only when `Player.CanSatisfyDebt(amountOwed) == true`
    - On click: Closes UI (and resumes payment flow in Liquidation Mode)
- **Declare Bankruptcy Button** (Liquidation Mode Only):
    - Always enabled
    - On click: Player declares bankruptcy and is eliminated
    - Confirmation prompt should be here

**Dismissal**:
- Standard Mode: Player closes manually via Close button
- Liquidation Mode: Closes only when sufficient funds raised OR player declares bankruptcy

**Implementation Notes**:
- Can be implemented incrementally (upgrades first, mortgage later)
- Liquidation Mode primarily adds: debt display, filters available actions, conditional Close button
- Properties should show clear visual state: mortgaged (grayed/marked), improvement level (house icons)
- Action buttons should show resulting money change (e.g., "Mortgage (+$100)" or "Add House (-$50)")

---

[_Return to Top_](#ui-functional-requirements)

---

## 5. Dice Roll UI

**Purpose**: Allow player to roll dice and initiate movement during their turn

**Trigger**: Button press from UI for player, `TurnStartedEvent` for AI

**Information Display**:
- "Roll Dice" prompt or instruction
- Current player indicator (whose turn it is)
- Dice animation/result display after roll
- **After roll**: Movement indication (spaces to move)

**Interactive Elements**:
- **Roll Dice Button**:
    - Enabled when: `Player.CanRollDice() == true` (hasn't rolled this turn, not in special state)
    - Disabled when: Already rolled this turn, or waiting for other actions to complete
    - On click: Calls `Player.ExecuteRollDice()`, triggers dice animation and movement
- **Optional**: Dice display area showing roll result (e.g., two dice showing pips)

**Dismissal**: Automatically dismisses/transitions after dice are rolled and movement completes. Non-blocking UI, can open Property Manager while dice rolling.

**Implementation Notes**:
- Already implemented UI and backend
- May need minor modifications refactor into PlayerController pattern
- AI players should have brief delay before auto-rolling (for visual clarity)

---

[_Return to Top_](#ui-functional-requirements)

---