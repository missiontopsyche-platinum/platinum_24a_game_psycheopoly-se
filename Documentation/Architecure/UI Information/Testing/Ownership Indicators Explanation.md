# Title: Ownership Indicators Explanation

**Author(s):** Name(s)
**Date Created:** 00/00/0000
**Last Updated:** 03/18/2026
**Version:** x.x

---

## Purpose
Initial thoughts for ownership indicators

## Scope
UI ownership indicators

## Related Documents
- Links to any other documents like README, UML, architecture explanations, etc.

## Notes
None at this time

Initial thoughts for ownership indicators (rough draft):

- OPT 1: Owned properties may show a colored border matching the owning player color.
	- Could also do a colored square in the corner if a border is going to be too visually "loud"
	  especially later in the game
- OPT 2: Owned properties may have a dot on the colored-space banner, OnHover will display ownership 
  info
	- look at PDF for game artwork that contains the white rectangles with a dot in the center
	- ownership info will not be obvious until the user hovers over the ownership icon on a space
- Unowned properties show no indicator
- Indicator should not block text or artwork
- Must be visible at default zoom level
- Ownership indicators should remain consistent across all properties, and shouldn't "take-over" the
  provided artwork on the board
- Ownership indicators should use small shapes, and LIMITED color to not over complicate the visuals
  of the board
- Indicators should have a flexible enough backend integration so transfer of ownership/sale of
  properties can be done easily

Final Decision for US577:
- OPT 2: Owned properties may have a dot on the colored-space banner, OnHover will display ownership 
  info
*** EDIT ***
	- upon recognizing the use of OnHover for visibility of Board Spaces, the Ownership details
	  will be visible within that Board Space OnHover event. this will keep the UI from being
	  visually overwhelming, as well as keeping the UI cohesive and simple to understand. the 
	  presence of the icons on the board space will remain, however
*** EDIT ***
	- look at PDF for game artwork that contains the white rectangles with a dot in the center
		- these specific graphics might not be used, but the concept will
	- ownership info will be represented by small icon;user hovers over the ownership icon on a space
	  to actually see the ownership info
		- this will allow user to hover over a space and get more details
		- users won't have a significantly obstructed/complicated view, requires hover to 
		  see details

	- when the user hovers over the icon, this display will pop up

--------------------------------------------
--------------------------------------------
----- Owned Property : [Property Name] -----
---- ---- ---- ---- ---- ---- ---- ---- ----
------------ Owned By : [user] -------------
--- Ownership Status : [Owned/Mortgaged] ---
----- Upgrade Level : [upgrade level] ------
--------------------------------------------
--------------------------------------------






