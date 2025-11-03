# Platinum 24A - Game - Psyche-opoly

ASU SER40X Capstone Project

WIP.

## To Get Set Up

1. Clone this repository to your local machine
2. Download and install the [Unity Hub](https://unity.com/download)
3. Ensure you have version `6000.2.6f1` installed _(subject to change)_
4. Go to Projects from the left-side tab
5. Click the "Add" menu next to the "New Project" button
6. Select "Add project from disk"
7. Navigate to your repository folder (should be `platinum_24a_game_psycheopoly-se` 
   and click "Open"
8. Unity will fill out the missing package and engine resources that are not being 
   tracked

This project uses the Unity Code Coverage tool. To ensure that it is set up to
generate a coverage report when tests are run, click from the top menu in the Unity
Editor, `Window/Analysis/Code Coverage`, and check the "Enable Code Coverage" box.
Currently, it is set to not open the coverage report automatically, but that
report can be found at 
[`CodeCoverage/Report/index.html`](CodeCoverage/Report/index.html).

To ensure that the Testing Framework is set up correctly for your local machine,
a handful of simple tests were created. Navigate to `Window/General/Test Runner`
and select "Run All" from the bottom-right corner. As we develop more tests, these
'sanity tests' will cease to be useful.

## File Structure

### `Assets/`

General file structure within the `Assets/` folder is as such:
- `Materials/` - Location of Material and Shader assets
- `Prefabs/`
  - `Board/` - Location of Board specific prefabs for rendering
  - `HUD` - Location of HUD UI prefab elements
  - `Managers` - Location of Manager prefabs with pre-hooked event channels
  - `Misc/` - Location of miscellaneous prefab objects as needed, such as Unity 
              scene basics.
- `Resources/`
  - `Dice` - Location of Dice image assets
  - `EventChannels/` - Location of EventChannel assets
  - `Logging/` - Location of LogSettings asset
- `Scenes/` - Location of Scene assets
- `Scripts/` 
  - `Board/` - Location of Board-related scripts
  - `Data/` - Location of data structure classes and scripts
  - `Events/` - Location of Event ScriptableObject definitions
    - `EventChannelTypes/` - Location of EventChannel concrete definitions for
                            asset creation
    - `EventChannelDataStructures/` - Location of data payloads for Events
  - `Interactable/` - Location of mouse interaction interfaces for rendered game
  - `Logging` - Location of Logging implementation and documentation
  - `Managers/` - Location of Manager scripts
  - `UI/` - Location of UI related scripts and controllers
- `Settings/` - Location of project settings
- `Tests/`
  - `EditMode/` - Location of EditMode (compile time) tests
  - `PlayMode/` - Location of PlayMode (run time) tests
  - `Shared/` - Location of shared test base classes
  
### `Documentation/`

Contains documentation and artifacts associated with this project.

### `Packages/`

Package manifest for this project so Unity can resolve dependencies automatically.

### `ProjectSettings/`

Contains all the Unity project settings for this project.