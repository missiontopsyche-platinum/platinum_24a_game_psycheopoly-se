# AI Property Purchase Decision System

---

## Overview

The AI uses a weighted scoring system to decide whether to purchase properties 
when landed upon. This creates emergent behavior across different game phases 
without needing to hard code rules for the early versus late game.

The key design factors in determining this weighting system were:

- Aggressive purchasing in the early-game (more money = more likely to buy)
- Strategic purchasing towards the mid-game (prioritize owning sets)
- Conservative in late-game (Only critical purchases)
- Randomness prevents 'perfect' behaviour and adds variation
- Tunable weights via ScriptableObjects

---

### Contents

- [Decision Flow](#decision-flow)
- [Overview of Scoring Factors and Fields](#overview-of-scoring-factors-and-fields)
- [Other Configurable Fields](#other-configurable-fields)
- [Detailed Description of Factor Checks](#detailed-description-of-factor-checks)
- [AI 'Personality' Variants](#ai-personality-variants)
- [Implementation Notes](#implementation-notes)
- [Complete Scoring Examples](#complete-scoring-examples)

---

## Decision Flow

1. AI lands on unowned property
2. Calculate Purchase Score (sum of weighted factors)
3. Calculate Adjusted Threshold (based on current wealth and random variation)
4. Compare Score versus Threshold (`Score >= Threshold`)
    1. If `YES`, purchase property
    2. If `NO`, decline purchase

## Overview of Scoring Factors and Fields

| Factor               | Purpose                                                                                  | When Applied                             | Default Weight |
|----------------------|------------------------------------------------------------------------------------------|------------------------------------------|----------------|
| Base Value           | Every property has inherent value                                                        | Always                                   | +10            |
| Reserve Cushion      | Rewards having cash left over                                                            | Always (or veto if < min)                | +0-20          |
| Color Group Progress | Rewards building towards sets                                                            | Per property owned in group              | +25 each       |
| Monopoly Completion  | Rewards creating a monopoly specifically, to avoid strategic mistakes through randomness | Only when completing a set               | +50            |
| High-Value Property  | Late-game high-value property bonus                                                      | Property value >= "High Value Threshold" | +10            |

## Other Configurable Fields

| Field Name                    | Purpose                                                           | Default Value |
|-------------------------------|-------------------------------------------------------------------|---------------|
| Minimum Reserve               | Percentage of Starting Cash that the AI will never purchase below | 15%           |
| Base Purchase Threshold       | Threshold for determining if a purchase is made                   | 50            |
| Random Variance               | The +/- range randomized and summed with Purchase Threshold       | 20            |
| High-Value Property Threshold | Buy price of Properties deemed 'high value'                       | $300          |
| Wealth Threshold Reduction    | Maximum threshold reduction at 100% wealth                        | -30           |

---

[_Return to Top_](#ai-property-purchase-decision-system)

---

## Detailed Description of Factor Checks

### Minimum Reserve

Before any scoring, check if the purchase is affordable.
```
Minimum Reserve = Starting Cash * Minimum Reserve Percentage

If (Player Cash - Property Price) < Minimum Reserve:
    return Score = -999 (hard veto)
```
This prevents the AI from bankrupting itself or putting it in a position where
one or two turns of paying rent will end its game.

### Base Property Value

All properties start with a base score.
```
Score += Base Property Value (weight defined in ScriptableObject)
```

### Reserve Cushion Weight

Rewards the AI for having cash left over after the purchase.
```
Cash After Purchase = Player Cash - Property Price
Wealth Ratio = Cash After Purchase / Starting Cash

Score += Wealth Ratio * Reserve Cushion Weight
```

**Example:**
- Player has $1200, property costs $100. Starting cash was $1500 (default).
- Reserve Cushion Weight is set to `20`.
- After purchase: $1100
- Wealth Ratio: $1100 / $1500 = 0.733
- Score = 0.733 * 20 = **+14.7 points**

### Color Group Progress Weight

Rewards building towards monopolies.
```
Properties owned in the same color group = N

Score += N * Color Group Progress Weight
```
**Example:**
- Player owns 2 properties in Maroon group, color progress weight is 25.
- Landing on a third Maroon property
- Score = 2 * 25 = **+50 points**

### Monopoly Bonus

Binary bonus for completing a monopoly, to guard against random chance that the
AI will decline to complete a set.
```
If (Properties Owned in Group) == (Total Properties in Group - 1):
    Score += MonopolyCompletionBonus
```
**Example:**
- AI owns 2/3 properties in Yellow group
- Landing on 3rd Yellow property
- This would complete monopoly: **+50 points**

### High-Value Property Bonus

Adds a small bonus for expensive properties that have high late-game impact.
```
If Property Price >= High-Value Property Threshold:
    Score += High Value Property Bonus
```

### Wealth Adjusted Threshold

The threshold for purchasing a property drops when the AI is wealthy, making
purchases in the early-game easier/more frequent. This effect wears off over the
game as purchases are made, but returns to being easier when wealth snowballs in
late-game.
```
Wealth Ratio = Player Cash / Starting Cash
Wealth Adjustment = Wealth Threshold Reduction * Weight Ratio

Final Threshold = Base Purchase Threshold + Wealth Adjustment + Random Variance
```

**Example Thresholds using Default Values**

| Player Cash | Wealth Ratio | Adjustment | Base | Random | Final Range      |
|-------------|--------------|------------|------|--------|------------------|
| $1500       | 1.00         | -30        | 50   | +/- 20 | **[0, 40]**      |
| $900        | 0.60         | -18        | 50   | +/- 20 | **[12, 52]**     |
| $450        | 0.30         | -9         | 50   | +/- 20 | **[21, 61]**     |
| $225        | 0.15         | -4.5       | 50   | +/- 20 | **[25.5, 65.5]** |

---

[_Return to Top_](#ai-property-purchase-decision-system)

---

## AI 'Personality' Variants

AI Personality can be varied by creating new AI weight assets from Scriptable Objects.
Tuning the weights and values used in making decisions can create distinct AI behaviors.
For example:

### Aggressive AI
```
minimumReservePercent: 0.10 (risky, only 10% reserve)
wealthThresholdReduction: 40 (very low threshold when rich)
basePurchaseThreshold: 40 (easier to trigger)
monopolyCompletionWeight: 60 (really pushes for sets)
randomnessRange: 25 (more unpredictable)
```
**Behavior:** Buys aggressively throughout the game, takes more risks

### Cautious AI
```
minimumReservePercent: 0.20 (safe, 20% reserve)
wealthThresholdReduction: 20 (less aggressive even when rich)
basePurchaseThreshold: 60 (harder to trigger)
randomnessRange: 15 (more predictable)
```
**Behavior:** Conservative buyer, very selective late game

### Balanced AI (Default)
```
minimumReservePercent: 0.15 (moderate)
wealthThresholdReduction: 30 (standard)
basePurchaseThreshold: 50 (middle ground)
randomnessRange: 20 (moderate variance)
```
**Behavior:** Standard buying pattern with natural game phases

---

[_Return to Top_](#ai-property-purchase-decision-system)

---

## Implementation Notes

### Scriptable Object Architecture
- Each AI 'personality' is a separate `.asset` file
- Can be swapped at runtime to change AI behavior or configured pre-game
- No code changes to tune behavior

### Integration with `AIPlayerController`
`AIBehaviorWeights` ScriptableObject is stored on construction in AIPlayerController.
This can be called from AIPlayerController to get weights to execute behavior.

### Required Player Data
The AI needs access to from the Player API:
- Player Money
- Player Owned Properties
- Number of Properties owned in a color group

### Debugging
The `ShouldPurchase` implementation includes a Logger entry for its decision:
```
Player X: AI Purchase Evaluation: {PropName} | Score: XX | Threshold: XX | Decision: T/F
```

---

[_Return to Top_](#ai-property-purchase-decision-system)

---

## Complete Scoring Examples

### Scenario 1: Early Game, First Property
```
Player State:
- Cash: $1500
- Properties Owned: 0

Property:
- Name: Hebe
- Price: $60
- Type: Property (Yellow group)
- Color Group Status: 0/3 owned

Scoring:
- Base Value: 10
- Reserve Cushion: ($1440 / $1500) × 20 = 19.2
- Color Group Progress: 0 × 25 = 0
- Monopoly Completion: 0
- High-Value: 0
Total Score: 29.2

Threshold:
- Base: 50
- Wealth Adjustment: -30 × 1.0 = -30
- Random: +/- 20
Final Threshold Range: [0, 40]

Decision: VERY LIKELY BUY (score 29.2 vs threshold 0-40)
```

### Scenario 2: Mid Game, Completing Monopoly
```
Player State:
- Cash: $800
- Properties Owned: 5 (including 2/3 Red group)

Property:
- Name: Thisbe
- Price: $180
- Type: Property (Red group)
- Would Complete Monopoly: YES

Scoring:
- Base Value: 10
- Reserve Cushion: ($620 / $1500) × 20 = 8.3
- Color Group Progress: 2 × 25 = 50
- Monopoly Completion: 50
- High-Value: 0
Total Score: 118.3

Threshold:
- Base: 50
- Wealth Adjustment: -30 × 0.533 = -16
- Random: +/- 20
Final Threshold Range: [14, 54]

Decision: DEFINITELY BUY (score 118.3 >> threshold)
```

### Scenario 3: Late Game, Low Cash
```
Player State:
- Cash: $300
- Properties Owned: 8

Property:
- Name: Egeria
- Price: $100
- Type: Property (Mustard group)
- Color Group Status: 0/3 owned

Scoring:
- Base Value: 10
- Reserve Cushion: ($200 / $1500) × 20 = 2.7
- Color Group Progress: 0 × 25 = 0
- Monopoly Completion: 0
- High-Value: 0
Total Score: 12.7

Threshold:
- Base: 50
- Wealth Adjustment: -30 × 0.20 = -6
- Random: +/- 20
Final Threshold Range: [24, 64]

Decision: UNLIKELY BUY (score 12.7 << threshold)
```

---

[_Return to Top_](#ai-property-purchase-decision-system)

---