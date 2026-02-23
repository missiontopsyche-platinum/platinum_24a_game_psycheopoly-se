-----US578 – T635-----
UI Icon Visibility Testing Scene (in testing environments to be sure to not affect gameplay scenes)

-----Purpose-----
This scene exists to visually verify that finalized UI icons remain clearly visible on both light and dark backgrounds.

It supports Task 635 by allowing direct side-by-side comparison of icons under:
- Light UI panels
- Dark UI panels


-----Current State-----
The scene layout is complete and includes:
- full-screen Canvas
- Light & Dark comparison panels
- grid containers for icon placement

Owned/Mortgaged sprites not assigned yet because the finalized assets from US577 were not pushed at the time of implementation. Once pushed, this should be testable for any/all UI elements as seen fit.

-----How To Extend-----
This test scene can simply be expanded in the future by:
- adding additional UI icons to the grid
- testing icons at multiple resolutions
- adding panels that simulate additional UI contexts (hover panels, overlays, popups, etc.)