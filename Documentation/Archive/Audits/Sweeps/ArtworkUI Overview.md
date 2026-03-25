All board space and card artwork used is stored under Assets>Artwork folder, in their respective subfolder, and assigned through the Unity Inspector. 

>> ScriptableObjects (BoardSpaceArt, CardArt, DataCards) contain references to artwork using an Artwork type field, which allows each space/card to define its own visual asset independently of the UI logic. 

>>UI components (BoardRenderer, SpaceRenderer, and CardPopupUI) read these ScriptableObject references at runtime and display the appropriate image when rendering spaces or cards. If no artwork is assigned, the system falls back to the default colored tile visuals to ensure the board remains functional and readable. This structure keeps artwork, data, and UI behavior loosely coupled and allows new artwork to be added or changed without modifying code.

>>For now, the implementation of DataCards is not complete, although the artwork is uploaded and assigned through the Unity Inspector. The reason behind this decision is we may end up changing specific cost/payment analysis, and the artwork would be rendered obsolete. If we DO end up removing those DataCard artwork assets from implementation, we should also remove the DataCard folder, and all related data within for optimal memory usage.

>> LASTLY, the visuals WILL appear blurry upon opening the game. This is a noteworthy issue. I've researched it and a few things to note:
	- If, while in PlayMode, the default Game view resolution (Free Aspect) causes the visuals to appear very blurry and hard to
	  read. When switching the Game view to 4K UHD, the board spaces appear much clearer (and are even MORE clear when zooming in). 	  This gives me reason to believe (after some research) the artwork itself isn't low quality/resolution, but rather a problem 		  with how Unity is distributing pixels across the game in the current view, leading un-zoomed/default views to appear blurry