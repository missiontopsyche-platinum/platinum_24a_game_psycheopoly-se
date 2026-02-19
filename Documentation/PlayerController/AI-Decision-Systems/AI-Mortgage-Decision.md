# AI Mortgage Decision System

---

## Overview

The AI uses a deterministic scoring system to gather the least detrimental properties to
mortgage in the event of being below a danger threshold. This allows the AI to make
intelligent decisions when in a situation where liquidating assets is necessary.

This system is fully deterministic and does not rely on weights to influence the
scoring, therefore it does not require a special weights class like the other
behaviors.

---

## Contents

1. [Class Structure](#class-structure)
2. [Decision Flow](#decision-flow)
3. [Thresholds](#thresholds)
4. [Property Pooling](#property-pooling)
5. [Scoring](#scoring)
6. [Candidate Selection](#candidate-selection)

---

## Class Structure

This behavior is significantly more complicated than others, and requires extra 
class components to function as needed.

| Class Name             | Description                                                                                                                                                                                         |
|------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `AIMortgageBehavior`   | The behavior class itself. Constructor takes `Player` and `MortgageThresholds`. Single public method `EvaluateMortgage()` returns `AIMortgageEvaluation`. Everything else is private scoring logic. |
| `AIMortgageEvaluation` | The result object. Public properties: `Queue<MortgageAction>`, `String Message` for testing. No behavior, just data.                                                                                |
| `MortgageAction`       | Public properties: `PropertySpaceData`,` MortgageActionType` enum, `int ExpectedCashGain`.                                                                                                          |
| `MortgageThresholds`   | Collection of thresholds used to finalize evaluations for property actions. Contained within the `AIBehaviorWeights` `ScriptableObject`.                                                            |
| `MortgageActionType`   | Enum: `SellDataPoint`, `SellDiscovery`, `Mortgage`, in the future `Unmortgage` when that behavior is implemented.                                                                                   |

---

[_Return to Top_](#ai-mortgage-decision-system)

---

## Decision Flow

1. Find out how much cash the player currently has (can be negative): "Projected Cash".
2. Establish the Mortgage Pool (properties without upgrades or group upgrades)
3. Establish the Sell Pool (local max-level properties within their respective groups)
4. Set up Mortgage Action queue for the selection loop
5. Loop: While the Projected Cash is less than the Danger Threshold, do:
   1. Score all the candidates in both pools
   2. Select the highest value-ratio candidate
   3. Add Mortgage Action to Queue
   4. Add the expected cash return to the Projected Cash value
   5. If the action was to sell an upgrade:
      1. Remove the property from the Sell Pool
      2. If the group still has upgrades, add the next Local Max Level 
         Property(ies) to Sell Pool
      3. If group has no upgrades remaining, add all properties from that 
         group to the Mortgage Pool
   6. If the Action was a Mortgage, remove the property from the Mortgage Pool
   7. If no candidates remain in either pool:
      1. If projected cash is less than or equal to 0, return with Actions 
         and message: "Bankrupt"
      2. If projected cash is greater than 0, return with Actions and message 
         "Below danger threshold"
6. Return AI Mortgage Evaluation (Actions, Message)

---

[_Return to Top_](#ai-mortgage-decision-system)

---

## Thresholds

| Field            | Purpose                                                              | Default Value |
|------------------|----------------------------------------------------------------------|---------------|
| DangerThreshold  | The cash floor the AI targets to reach through mortgaging actions    | $75           |

> Note: Unlike other AI behaviors, this system requires no weights. The scoring
> is fully determined by the game's own economic values: purchase prices,
> mortgage values, and rent tables rather than tunable factors. The single
> threshold controls when the behavior triggers and when it exits.

---

[_Return to Top_](#ai-mortgage-decision-system)

---

## Property Pooling

The behavior maintains two candidate pools during execution, both of which are dynamic and 
updated after each action.

### Mortgage Pool

Initialized at the start of evaluation with all owned properties that are eligible for mortgage.
A property is eligible if:
- It is not already mortgaged, AND
- It has no upgrades on it, AND
- No other property in its color group has upgrades

Properties are removed from the pool as they are selected for mortgage. Properties are
added to the pool mid-execution when a color group's last upgrade is sold, promoting all
properties in that group to mortgage candidates.

### Sell Pool

Initialized at the start of evaluation with all properties that are at the local maximum
upgrade level within their color group. Multiple properties in the same group may be in the
pool simultaneously if they share the group's local maximum upgrade level.

The pool is updated after each sell action:
- The sold property is removed from the pool
- If the group still has upgrades, the next local max-level property or properties are added
- If the group has no upgrades remaining, all remaining group entries are cleared and all
  properties in that group are promoted to the Mortgage Pool

---

[_Return to Top_](#ai-mortgage-decision-system)

---

## Scoring

Each candidate in both pools is scored using a Value Ratio that represents how much cash
is raised relative to the long term cost. Higher ratios are preferred.
```
Total Cost = Recovery Cost + Current Income
Value Ratio = Cash Raised / Total Cost
```
Ratios are rounded to three decimal places to handle floating point imprecision. Candidates
with equal ratios after rounding are handled by the tiebreaker chain described in
[Candidate Selection](#candidate-selection).

### Cash Raised

| Action Type    | Cash Raised                                           |
|----------------|-------------------------------------------------------|
| Mortgage       | 50% of property purchase price                        |
| SellDataPoint  | 50% of Data Point cost                                |
| SellDiscovery  | 50% of Discovery cost  (four data points + discovery) |

### Recovery Cost

| Action Type    | Recovery Cost                                                   |
|----------------|-----------------------------------------------------------------|
| Mortgage       | 110% of mortgage value                                          |
| SellDataPoint  | Full purchase price of Data Point                               |
| SellDiscovery  | Full purchase price of Discovery (four data points + discovery) |

### Current Income

Current Income represents the income lost per turn as a result of the action.
Its calculation varies by space type:

| Space Type                 | Current Income                                                                    |
|----------------------------|-----------------------------------------------------------------------------------|
| Property (Mortgage)        | Base rent at upgrade level 0                                                      |
| Property (Sell Data Point) | Rent delta between current and previous upgrade level                             |
| Property (Sell Discovery)  | Rent delta between level 5 and level 0                                            |
| Instrument                 | Current rent tier based on number of instruments owned by player                  |
| Planet                     | The average 2d6 roll (7) * Planet Bonus (4 if one planet owned, 10 if both owned) |

---

[_Return to Top_](#ai-mortgage-decision-system)

---

## Candidate Selection

Candidates from both pools are scored each iteration and stored in a dictionary keyed
by their rounded Value Ratio, with each bucket containing a list of candidates that
share that ratio. The highest-keyed bucket is evaluated first. 

When selected, a Mortgage Action is created with that property and the details of its
action, and added to the Action Queue to be returned. The Mortgage and Sell pools are
then managed as defined in the [Property Pooling](#property-pooling) section.

The selection loop continues until Projected Cash meets or exceeds the Danger
Threshold. Each iteration selects the single highest scoring candidate from
the combined pool rather than pre-selecting a set of properties upfront,
ensuring the minimum number of sacrifices are made to reach the threshold.

### Tiebreaker Chain

When a bucket contains more than one candidate, the following tiebreaker chain is
applied in order until a single candidate is selected:

1. **Higher Cash Raised** - Prefer the action that raises more cash
2. **Lower Current Income** - Prefer sacrificing the lower income space
3. **Mortgage over Sell** - Prefer mortgaging over selling upgrades

### Emergent Behavior

No explicit tier ordering is enforced between the two pools. However, the scoring
naturally produces a consistent preference ordering as an emergent property of the
game's economics:

1. Low value unimproved properties
2. High value unimproved properties
3. Low level Data Point sells
4. High level Data Point sells
5. Discovery sells

This ordering reflects the fact that upgrade income deltas consistently outpace
base rents, making improved properties more costly to sacrifice than unimproved ones
in almost all cases.

---

[_Return to Top_](#ai-mortgage-decision-system)

---
