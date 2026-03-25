# Title Board Layout Options US533

**Author(s):** Name(s)
**Date Created:** 00/00/0000
**Last Updated:** 03/18/2026
**Version:** 1.0

---

## Purpose
Explains the different ways the board can be layed out for the project along with the pros and cons for each. 

## Scope
The board layout logic/architecture

## Related Documents
- Links to any other documents like README, UML, architecture explanations, etc.

## Notes
None at this time

--------------------------Option 1: ScriptableObject-driven (current implementation[US517])

- board spaces are defined as data (ScriptableObjects), and the board is built from that data at runtime.
- how the board is already being implemented from US517, where board spaces and their properties are data-driven and rendered by the board/space renderer at runtime

Pros:
- Already partially implemented (US517)
- Board layout is driven by data instead of hard-coded scene objects
- Easier to maintain and update
- Makes it possible to support future board changes or themes
- Cleaner separation between board data and visuals

Cons:
- Requires some setup and structure
- Slight learning curve for newer team members
- Time consuming
- Currently, issue with blurriness due to resolution concerns (will require future effort)




--------------------------Option 2: Fully procedural layout

- this would generate the board entirely from a config or grid system at runtime.

Pros:
- More flexible
- Could support multiple board sizes and shapes (might enhance resolution issues)
- Scales well for future expansion

Cons:
- Most complex solution
- Higher bug risk
- Harder to debug
- Likely more than we need right now





--------------------------Option 3: Hybrid Option

- this essentially would build off of option1, with the overall goal to stop the 40 individual SpaceRenderer objects from generating at runtime (although their artwork, text, etc. is already set in the inspector for the SO's)
- basically, at runtime we are only binding the SpaceData to the SpaceRenderer's, ultimately still keeping the SetUpSpace(data, scale), but without Instantiate() and GetPositionSpace()
- i did research on this so it still needs some thought but the goal would be to stop instantiating each space prefab in a loop at runtime, stop computing positions at runtime, and we wouldn't be getting spaces rotated at runtime (if we decide to rotate spaces [still up for discussion]).
- this would be done by placing the 40 SpaceRenderer objects in Unity (in a scene or prefab) ahead of time so they'll exist in the hierarchy outside/before PlayMode

Pros:
- Keeps a good chunk of Option 1 (artwork, text, etc. is already loaded in the inspectors)
- Less runtime complexity
- Theoretically would be easier to debug than option 2 (more visual checks)
- Should be faster at runtime

Cons:
- Would require us to edit scene/prefabs
- Still going to hit visual/resolution issues





-------------------------Recommendation (initial)

Continue with the ScriptableObject-driven board layout approach already started in US517.

It is flexible, and already partially delivered. The poor-resolution concern IS going to need to be addressed, but I'm almost certain it would be less expensive than going fully proceedural
