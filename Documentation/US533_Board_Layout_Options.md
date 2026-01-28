US533 – Board Layout Options


--------------------------Option 1: ScriptableObject-driven (current implementation[US517])

- board spaces are defined as data (ScriptableObjects), and the board is built from that data at runtime.
- how the board is already being implemented from US517, where board spaces and their properties are data-driven and rendered by the board/space renderer.

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



-------------------------Recommendation (initial)

Continue with the ScriptableObject-driven board layout approach already started in US517.

It is flexible, and already partially delivered. The poor-resolution concern IS going to need to be addressed, but I'm almost certain it would be less expensive than going fully proceedural
