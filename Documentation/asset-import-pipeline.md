# Asset Import Pipeline

## Purpose
This document will define the asset import pipeline to make sure there is visual consistency, maintainability, and standard files across all art and UI assets that are added to the project.  This pipeline will apply to all new assets added to the repo and will be included in the documentation section of the repo as well. 

---

## 1. Approved Formats, Compressions, and Max Sizes

### 1.1 Image Formats

| Asset Type | Format | Notes |
|----------|--------|-------|
| UI, Icons, Cards | PNG | Lossless, supports transparency |
| Board and Space Art | PNG | Consistent color fidelity |
| Temporary and Prototype | PNG | JPG should be avoided if possible |
| Vector Source | SVG | Not imported directly into Unity |

**JPG and GIF not allowed in Repo

---

### 1.2 Texture Compressions (Unity)

**UI and Cards**  
- Compression: None  
- Filter Mode: Bilinear  
- Mip Maps: Off  

**Board or World Art**  
- Compression: NOrmal  
- Filter Mode: Bilinear  
- Mip Maps: Off  

---

### 1.3 Max File Size Guidelines

| Asset Type | Max Size |
|-----------|----------|
| UI Icon | Less than 256 KB |
| Cards | Less than 1 MB |
| Board Spaces | Less than 2 MB |
| Full Board Art | Less than 4 MB |

---

## 2. Naming Conventions

### 2.1 General Rules
- PascalCase for asset names  
- No spaces  
- No special characters  
- Prefix with asset type for prefabs  

---

### 2.2 Prefix Standards

| Asset Type | Prefix | Example |
|----------|--------|---------|
| UI Prefab | UI_ | UI_TurnBanner |
| Button | Btn_ | Btn_Primary |
| Text | Text_ | Text_H1 |
| Card Data | Card_ | Card_Metal_AdvanceToCeres |
| Card Deck | CardDeck_ | CardDeck_Metal |
| Card Effect | Verb based | PayPlayer_50 |
| Board Space Art | Name only | CERES |
| Data Card Art | _DATACARD | CERES_DATACARD |

**Avoid: final, new, temp, v2, copy in naming

---

## 3. Target Resolutions

| Asset Type | Resolution |
|----------|------------|
| UI ICons | 128 x 128 or 256 x 256 |
| Buttons | 256 x 64 or 256 x 80 |
| Card Art | 1024 x 1024 |
| Data Cards | 1024 x 1536 |
| Board Spaces | 512 x 512 |
| Dice | 256 x 256 |
| Full Board | |

**All assets should be power of two when possible

---

## 4. Unity Import Settings Guidelines

### 4.1 UI Assets
- Texture Type: Sprite (2D and UI)  
- Pixels Per Unit: 100  
- Generate Mip Maps: Off  
- Wrap Mode: Clamp  

---

### 4.2 Board or World Art
- Texture Type: Sprite (2D and UI) or Default  
- Pixels Per Unit: 100  
- Mip Maps: Off unless zoomed in  

---

### 4.3 Scriptable Objects
- Stored under Assets/Resources/…  
- Do not rename or move without code review  
- Naming must match asset reference expectations  

---

## 5. Template Folder Structure

```
Assets/ 
	Artwork/
		BoardSpaces/
		Cards/
		DataCards/
		Dice/
		Misc/ 
	Fonts/
	Prefabs/ 
		UI/ 
			Buttons/
			Text/
			HUD/
			Panels/
		Board/
	Resources/
		Card/
			Cards/
			Decks/
			Effects/
	UI/
		Theme/ 
```

**Misc. is meant for any artwork that is used in one location or single images that do not fit into the main categories listed in the Artwork folder. 

---

## 6. Sample Assets following the Pipeline

**Example of Compliant Assets:**
- CERES.png within folder Assets/Artwork/BoardSpaces/  
- Card_Metal_AdvanceToCeres.asset within folder Assets/Resources/Card/Cards/Metal/  
- Btn_Primary.prefab within folder Assets/Prefab/UI/Buttons/  
