\# Task 614 

\#Tile Scale and Orientation Validation 



\## Import standards (Tile Artwork)

* Texture Type: Sprite (2D and UI)
* Sprite Mode: Single
* Pixels Per Unit (PPU): 400
* Pivot: Center
* Wrap Mode: Clamp
* Filter Mode: Point (no filter)
* Compression: None
* Max Size: 2048



\##In Game Visual QA Checklist

Verified in Playmode:

* No tile art is rotated or flipped incorrectly 
* No tile art is stretched or squished when compared to reference tiles
* Tile art alignment is consistent over the entire board



\##Notes

* Space rendering uses Quad and MeshRender with the SpaceRender prefab
* Any future tile's that are added into the game should use the same standards above to make sure there is a consistent scale throughout the whole project 
