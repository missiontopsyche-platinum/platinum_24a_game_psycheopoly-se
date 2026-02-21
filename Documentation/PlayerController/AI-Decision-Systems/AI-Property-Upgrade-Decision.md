# AI Property Upgrade Decision System

## Overview

The AI uses a weighted scoring system to decide whether to upgrade properties
during its turn. This creates emergent spending behavior without needing to
hard code rules for when or how aggressively to upgrade.

The key design factors in determining this weighting system were:

- Prioritize high-ROI upgrades (first Data Points have the most impact)
- Self-regulating spending within a turn (reserve cushion decays with each purchase)
- Conservative floor to prevent bankruptcy (hard reserve veto)
- Randomness prevents 'perfect' behavior and adds variation
- Tunable weights via ScriptableObjects

## Contents

- [Decision Flow](#decision-flow)
- [Overview of Scoring Factors and Fields](#overview-of-scoring-factors-and-fields)
- [Detailed Description of Factor Checks](#detailed-description-of-factor-checks)
- [Complete Scoring Examples](#complete-scoring-examples)
- [AI Personality Variant Examples](#ai-personality-variants)

## Decision Flow

1. Get valid upgrade targets from `Player.GetValidUpgradableProperties()`
2. Score each candidate property
3. Calculate individual threshold for each candidate (base + random variance)
4. Filter to candidates whose score meets or exceeds their threshold
5. Upgrade the highest scoring candidate
6. Repeat from step 1 until no candidates qualify or hard veto is triggered

## Overview of Scoring Factors and Fields

| Factor/Field           | Purpose                                                                                                | When Applied                            | Default Value             |
|------------------------|--------------------------------------------------------------------------------------------------------|-----------------------------------------|---------------------------|
| Base Upgrade Score     | Represents the intrinsic value of any upgrade                                                          | Always                                  | 20                        |
| Upgrade Level Weight   | Scales Base Upgrade Score to subtly favor earlier upgrades                                             | Per candidate, indexed by current level | [1.2, 1.1, 1.0, 0.9, 0.8] |
| ROI Weight             | Rewards upgrades with the highest rent gain relative to the property's own maximum possible rent delta | Always                                  | 30                        |
| Reserve Cushion Weight | Rewards having cash remaining after upgrade via sigmoid curve, decays aggressively as cash runs low    | Always                                  | 30                        |
| Minimum Reserve        | Hard veto — AI will not upgrade if cash after upgrade falls below this value                           | Always                                  | $300                      |
| Base Upgrade Threshold | Minimum score required to trigger an upgrade                                                           | Always                                  | 40                        |
| Random Variance        | +/- range randomized and summed with Base Upgrade Threshold                                            | Always                                  | 20                        |

---

[_Return to Top_](#ai-property-upgrade-decision-system)

---

## Detailed Description of Factor Checks

### Minimum Reserve (Hard Veto)
Before any scoring, check if the upgrade is affordable.
```
Minimum Reserve = $300 (configurable)

If (Player Cash - dataPointCost) < Minimum Reserve:
    return false (hard veto)
```
This prevents the AI from leaving itself in a position where one or two rent
payments could end its game.

### Base Upgrade Score and Upgrade Level Weight
Represents the intrinsic value of any upgrade, scaled by the current upgrade
level to subtly favor earlier upgrades without overriding the ROI signal from
actual game data.
```
Score += BaseUpgradeScore * Upgrade Level Weight[currentLevel]
```
**Default weights by level:**

| Current Level | Weight | Effective Score (Base 20) |
|---------------|--------|---------------------------|
| 0             | 1.2    | 24                        |
| 1             | 1.1    | 22                        |
| 2             | 1.0    | 20                        |
| 3             | 0.9    | 18                        |
| 4             | 0.8    | 16                        |

This provides a gentle nudge toward spreading early Data Points across properties
rather than fully upgrading one group, while still allowing high-ROI mid-level
upgrades to score competitively.

### ROI Weight
Rewards upgrades that generate the most rent relative to the property's own
maximum possible rent delta. Normalized so that the ROI Weight represents the
maximum possible ROI contribution, achieved only at the property's most
impactful upgrade level.
```
Max Delta = maximum rent delta across all upgrade levels for this property
Rent Delta = researchFundingValues[currentLevel + 1] - researchFundingValues[currentLevel]
Normalized ROI = Rent Delta / Max Delta

Score += Normalized ROI * ROI Weight
```
**Example:**
- Thisbe researchFundingValues: [$14, $70, $200, $550, $750, $950]
- Max Delta = $350 (level 2 → 3 has the largest rent increase)
- Current Level: 0, Rent Delta = $70 - $14 = $56
- Normalized ROI = $56 / $350 = 0.16
- Score = 0.16 * 30 = **+4.8 points**

**Example at peak ROI:**
- Current Level: 2, Rent Delta = $550 - $200 = $350
- Normalized ROI = $350 / $350 = 1.0
- Score = 1.0 * 30 = **+30 points** (maximum possible ROI contribution)

### Reserve Cushion Weight
Rewards the AI for having cash remaining after the upgrade using a sigmoid
curve. This creates aggressive score decay as cash runs low, causing the AI
to self-regulate its spending within a turn without any explicit per-turn
upgrade limit.

The function will be normalized by taking the minimum and maximum values
from the Sigmoid function at instantiation time.
```
Reserve Ratio = (Player Cash - dataPointCost) / Starting Cash
Sigmoid Value = 1 / (1 + e^(-10 * (Reserve Ratio - 0.5)))
Normalized = (Sigmoid Value - Sigmoid Min) / (Sigmoid Max - Sigmoid Min)

Score += Sigmoid(Normalized) * Reserve Cushion Weight
```
The sigmoid function (k=10, centered at c=0.5) creates an S-curve that:
- Penalizes low reserve ratios heavily (< 0.3 produces minimal score)
- Rewards moderate ratios substantially (0.5-0.7 range)
- Plateaus at high ratios (> 0.8 gives diminishing returns)

> Note: The K and C values will be defined as constants in the behaviors source, in case
> modifications are desired.
> `K` defines the steepness of the curve. Low values have a shallower curve, high values are
> a steeper curve. `C` defines the center point of the inflection on the `x` axis. `0.5` centers
> the inflection, and because our `x` values are mapped from 0 -> 1, `c` must be between 0.0 and 1.0
> for the curve to be impacted at all.
> 
> To test K and C values over the range `[0, 1]` and see their effect on the Sigmoid curve,
> check out [this Desmos graph](https://www.desmos.com/calculator/fwdgq0fvf5).

**Example reserve scores:**

| Cash After Upgrade | Reserve Ratio | Sigmoid Output | Score Contribution |
|--------------------|---------------|----------------|--------------------|
| $1100              | 0.73          | 0.909          | 27.3               |
| $800               | 0.53          | 0.575          | 17.2               |
| $600               | 0.40          | 0.269          | 8.1                |
| $300               | 0.20          | 0.047          | 1.4                |

### Upgrade Threshold
The threshold a candidate must clear to trigger an upgrade.
```
Final Threshold = Base Upgrade Threshold + Random Variance

Random Variance = random value in range [-randomnessRange, +randomnessRange]
```
**Example Thresholds using Default Values:**

| Base Threshold | Random Range | Final Range |
|----------------|--------------|-------------|
| 40             | +/- 20       | [20, 60]    |

---

[_Return to Top_](#ai-property-upgrade-decision-system)

---

## Complete Scoring Examples

### Scenario 1: Early Turn, Cash-Rich, Level 0 Property
```
Player State:
- Cash: $1200
- Starting Cash: $1500

Property:
- Name: Thisbe
- Current Level: 0
- researchFundingValues: [$14, $70, $200, $550, $750, $950]
- dataPointCost: $100
- Max Delta: $350

Hard Veto Check:
- Cash After Upgrade: $1200 - $100 = $1100
- $1100 >= $300 minimum reserve: PASS

Scoring:
- Base Upgrade Score:  20 * 1.2 = 24
- ROI:                 ($56 / $350) * 30 = 4.8
- Reserve Cushion:     sigmoid(0.73) * 30 = 27.3
Total Score: 56.1

Threshold:
- Base: 40
- Random: +/- 25
Final Threshold Range: [15, 65]

Decision: LIKELY UPGRADE (score 56.1 vs threshold 15-65)
```

### Scenario 2: Mid-Turn, Cash Reduced, Level 0 Property
```
Player State:
- Cash: $400 (reduced from prior upgrades this turn)
- Starting Cash: $1500

Property:
- Name: Thisbe
- Current Level: 0
- researchFundingValues: [$14, $70, $200, $550, $750, $950]
- dataPointCost: $100
- Max Delta: $350

Hard Veto Check:
- Cash After Upgrade: $400 - $100 = $300
- $300 >= $300 minimum reserve: PASS (barely)

Scoring:
- Base Upgrade Score:  20 * 1.2 = 24
- ROI:                 ($56 / $350) * 30 = 4.8
- Reserve Cushion:     sigmoid(0.20) * 30 = 1.4
Total Score: 30.2

Threshold:
- Base: 40
- Random: +/- 25
Final Threshold Range: [15, 65]

Decision: UNLIKELY UPGRADE (score 30.2 vs threshold 15-65)
```

### Scenario 3: Level 2 to 3 Upgrade (Peak ROI), Moderate Cash
```
Player State:
- Cash: $900
- Starting Cash: $1500

Property:
- Name: Thisbe
- Current Level: 2
- researchFundingValues: [$14, $70, $200, $550, $750, $950]
- dataPointCost: $100
- Max Delta: $350

Hard Veto Check:
- Cash After Upgrade: $900 - $100 = $800
- $800 >= $300 minimum reserve: PASS

Scoring:
- Base Upgrade Score:  20 * 1.0 = 20
- ROI:                 ($350 / $350) * 30 = 30
- Reserve Cushion:     sigmoid(0.53) * 30 = 17.2
Total Score: 67.2

Threshold:
- Base: 40
- Random: +/- 25
Final Threshold Range: [15, 65]

Decision: CERTAIN UPGRADE (score 67.2 vs threshold 15-65)
```

### Scenario 4: Level 4 to Discovery, High Cash
```
Player State:
- Cash: $1000
- Starting Cash: $1500

Property:
- Name: Thisbe
- Current Level: 4
- researchFundingValues: [$14, $70, $200, $550, $750, $950]
- dataPointCost: $100
- Max Delta: $350

Hard Veto Check:
- Cash After Upgrade: $1000 - $100 = $900
- $900 >= $300 minimum reserve: PASS

Scoring:
- Base Upgrade Score:  20 * 0.8 = 16
- ROI:                 ($200 / $350) * 30 = 17.1
- Reserve Cushion:     sigmoid(0.60) * 30 = 20.7
Total Score: 53.8

Threshold:
- Base: 40
- Random: +/- 25
Final Threshold Range: [15, 65]

Decision: LIKELY UPGRADE (score 53.8 vs threshold 15-65)
```

---

[_Return to Top_](#ai-property-upgrade-decision-system)

---

## AI 'Personality' Variants

AI Personality can be varied by creating new AI weight assets from Scriptable Objects.
Tuning the weights and values used in making decisions can create distinct AI behaviors.
Here are some example personalities:

### Aggressive AI
```
minimumReserve: $200 (takes more risks)
baseUpgradeScore: 25 (upgrades more eagerly)
roiWeight: 35 (prioritizes efficient upgrades heavily)
reserveCushionWeight: 20 (less concerned about cash reserves)
baseUpgradeThreshold: 30 (easier to trigger)
randomnessRange: 30 (more unpredictable)
upgradeLevelWeight: [1.3, 1.2, 1.0, 0.8, 0.7] (stronger bias toward early upgrades)
```
**Behavior:** Upgrades aggressively and spreads early Data Points quickly, willing to run cash reserves lower

### Cautious AI
```
minimumReserve: $400 (very safe)
baseUpgradeScore: 15 (upgrades reluctantly)
roiWeight: 25 (less focused on efficiency)
reserveCushionWeight: 40 (very concerned about cash reserves)
baseUpgradeThreshold: 50 (harder to trigger)
randomnessRange: 20 (more predictable)
upgradeLevelWeight: [1.0, 1.0, 1.0, 1.0, 1.0] (no level bias)
```
**Behavior:** Conservative spender, only upgrades when cash-rich and conditions are favorable, spreads spending across turns

### High-Roller AI
```
minimumReserve: $250 (moderate risk)
baseUpgradeScore: 18 (slightly below default)
roiWeight: 40 (heavily prioritizes best ROI)
reserveCushionWeight: 25 (balanced)
baseUpgradeThreshold: 35 (moderate)
randomnessRange: 15 (more consistent)
upgradeLevelWeight: [1.0, 1.0, 1.0, 1.1, 1.2] (favors late upgrades and Discoveries)
```
**Behavior:** Chases peak ROI upgrades and Discoveries, less interested in spreading early Data Points

---

[_Return to Top_](#ai-property-upgrade-decision-system)

---