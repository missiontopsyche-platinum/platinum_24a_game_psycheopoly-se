# SpaceData Architecture

## Overview
SpaceData implements an event-driven architecture using ScriptableObjects for game data and 
EventChannels for decoupled communication.

## Core Pattern: Commands vs Queries

### Queries (Direct Calls)
Read-only operations that don't require validation:
```csharp
// Reading state - direct method calls
Color color = spaceData.spaceColor;
Player owner = ownableSpace.GetOwner();
```

### Commands (Events)
State-changing operations that require validation:
```csharp
// Changing state - goes through events
purchaseOwnableRequestEventChannel.Raise(
    new PurchaseOwnableRequestEvent(
        player, spaceData, cost));
```

**Why this separation?**
- GameManager acts as a mediator, running all actions through RuleSet strategies to be validated
- Simple read-only data requests do not need to be events, raises complexity and overhead when simple
  access methods can be used
- Rule of thumb: if there needs to be validation or a state change, use an EventChannel. If you just 
  need to query data, use direct method calls or property access

## Event Flows

> **NOTE:** These diagrams show the intended architecture for SpaceData interactions, and do not reflect
> final implementation of outside system communications (and are bound to change)

### Property Purchase
```mermaid
sequenceDiagram
   participant BoardManager
   participant OwnableSpaceData
   participant PurchaseOwnableRequestEventChannel
   participant GameManager
   participant Player
   participant RuleSet
   participant UI

   BoardManager->>OwnableSpaceData: OnLanded()
   OwnableSpaceData->>PurchaseOwnableRequestEventChannel: Raise(PurchaseOwnableRequestEvent)
   PurchaseOwnableRequestEventChannel->>GameManager: Handle Request
   GameManager->>RuleSet: ValidatePurchase(Player, OwnableSpaceData)

   alt Purchase Valid
      RuleSet->>GameManager: Valid
      GameManager->>OwnableSpaceData: SetOwner(Player)
      GameManager->>Player: Charge Player Buy Cost
      GameManager->>Player: Add ownable property to collection
      GameManager->>UI: Show player bought property message
   else Purchase Invalid
      RuleSet->>GameManager: Invalid(reason)
      GameManager->>UI: Show error message
   end
```

### Rent Collection
```mermaid
sequenceDiagram
    participant BoardManager
    participant OwnableSpaceData
    participant ChargeOwnershipFeeEventChannel
    participant GameManager
    participant RuleSet
    participant PlayerManager
    participant Player
    participant UI
    
    BoardManager->>OwnableSpaceData: OnLanded(Player)
    OwnableSpaceData->>OwnableSpaceData: Get Owner
    
    alt Property is owned by another player
        OwnableSpaceData->>ChargeOwnershipFeeEventChannel: Raise(ChargeOwnershipFeeEvent)
        ChargeOwnershipFeeEventChannel->>GameManager: Handle request
        GameManager->>Ruleset: CalculateRent(OwnableSpaceData, Player)
        RuleSet->>GameManager: Rent amount
        GameManager->>RuleSet: ValidatePayment(Player, Amount)
        
        alt Player can afford rent
            Ruleset->>GameManager: Valid
            GameManager->>PlayerManager: TransferMoney(fromPlayer, toOwner, amount)
            Player->>UI: Update Player balance
        else Player can not afford rent
            Note over GameManager,Player: Bankruptcy not designed yet
        end
    
    else Property is unowned or owned by current player
        OwnableSpaceData->>OwnableSpaceData: No rent due, continue turn.
    end
```

## Adding New Space Types

1. Create a new class inheriting from `SpaceData` (or `OwnableSpaceData` for properties)
2. Create a ScriptableObject asset file named by board position
    - Format: `{position}_{SpaceName}.asset`
    - Examples: `00_Go.asset`, `01_Hebe.asset`, `23_Psyche.asset`
    - **Note:** The asset filename doesn't need to match the class name - board position matters for validation
3. Implement `OnLanded()` to define space behavior
4. Use queries for reading state, raise events for state changes
5. Let GameManager handle validation and state updates

## Important Constraints

### ScriptableObject Class Files
Unity requires ScriptableObject **class definition files** to match their class names exactly:
- Correct: `PropertySpaceData.cs` contains `class PropertySpaceData`
- Wrong: `MySpace.cs` contains `class PropertySpaceData`

**Asset instances** (the `.asset` files) don't have this constraint - they're named by board position.

### Event Channel References
Event channels must be assigned to space instances. For bulk assignment across 40 spaces, we will
develop an editor script to go through each SpaceData and assign its EventChannel assets for easy
hookup.

In the meantime, manual entry is needed.

## Future Extensions
- Card system (see US #360)
- Alternate rulesets