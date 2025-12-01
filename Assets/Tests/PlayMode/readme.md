# Testing Infrastructure

## Overview

This folder contains the `EditMode` tests for the project. Tests are organized by
component with base classes that handle common `SetUp()`/`TearDown()` to reduce
boilerplate redundancy and increase maintainability.

## Folder Structure

```
Tests/EditMode/
├── PlayTestBase.cs              # Shared utilities for all play tests
├── README.md                    # This file
├── PieceTest                    # Tests Pieces. Per comments, does not fully function in play mode                  
├── BoardRenderer/
│   ├── BoardManagerTestBase.cs     # Setup/teardown for BoardRenderer
│   └── [specific test files]
├── CardEffects/
│   ├── CardEffectTestBase.cs      # Setup/teardown for GameManager
│   └── [specific test files]
├── HudControlPanels/
│   └── [specific test files]
└── TestCardUI/
```

## Base Class Pattern

### PlayTestBase

Most play tests should inherit from `PlayTestBase`, which provides:

- Logging Initalization                             : provided to avoid multiple declarations
- bool sceneLoaded.                                 : Used to track when the scene is fully loaded
- 'protected virtual OnSceneLoaded(Scene scene, LoadSceneMode mode)' : A necessary function to build objects after the scene is fully loaded
- "ClearAllPlayers()"                               : Function used to clear all players from game state. Necessary for any test that adds players.

The function OnSceneLoaded MUST INCLUDE the following line at the end:
- sceneLoaded = true; :  This is used to flag to your tests that the scene is loaded and ready for testing.

### Example

```csharp
public class BoardRendererTestBase : PlayTestBase
{
    protected GameObject boardGameObject;
    protected global::BoardRenderer boardRenderer;
    protected BoardManager boardManager;
    protected GameManager gameManager;
    protected Camera testCamera;
    protected GameObject spaceRendererPrefab;
    protected GameObject playerPiecePrefab;

    // event channels
    protected PlayerEventChannel testPlayerEventChannel;
    protected PlayerMovedEventChannel testMoveEventChannel;
    protected BooleanEventChannel testPieceMoveCompletedChannel;
    
    [SetUp]
    public virtual void SetUp()
    {
        //Create an on sceneLoaded event handler to build objects
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("PlayTestScene", LoadSceneMode.Single);
    }
    
    [TearDown]
    public virtual void TearDown()
    {
        SceneManager.UnloadScene("PlayTestScene");
    }

    // Retrieves necessary gameobjects after the scene is loaded. This is required by inherititing from PlayTestBase
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // set up camera using Scene Manager
        testCamera = GameObject.FindFirstObjectByType<Camera>();
        
        // get the board game object
        boardGameObject = GameObject.Find("Board");
       
        boardManager = boardGameObject.GetComponent<BoardManager>() as BoardManager;
        boardRenderer = boardManager.boardRenderer;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        testPlayerEventChannel = boardManager.playerAddedChannel;
        testPlayerEventChannel.Subscribe(boardRenderer.AddPlayerPiece);
        testMoveEventChannel = boardManager.playerMovedChannel;
        testMoveEventChannel.Subscribe(boardRenderer.MovePiece);
        testPieceMoveCompletedChannel = gameManager.pieceMoveCompletedChannel;

        // generate a test board
        SpaceData[] testSpaces = CreateTestSpaces(40);
        boardRenderer.GenerateBoard(testSpaces);

        //Ensures any players auto made by the playermanager are cleared so adding player tests
        //function correctly
        boardManager.ClearPlayers();
        boardRenderer.ClearPlayers();

        sceneLoaded = true;
     }
```


## Writing New Tests

### For a New Test Class

Every test should be set up as a Coroutine (IE: Returns and IEnumerator and is flagged as [UnityTest]). 
Each unit test MUST INCLUDE the following line to ensure the scene is loaded and game objects are fully grabbed.
-   yield return new WaitWhile(() => !sceneLoaded);

Example:

```csharp
using Logging;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Logger = Logging.Logger;
namespace Tests.PlayMode.BoardRenderer
{
    public class BoardRendererPlayerPieceTests : BoardRendererTestBase
    {
        [UnityTest]
        public IEnumerator AddPlayerPiece_CreatesNewPiece()
        {
            yield return new WaitWhile(() => !sceneLoaded);
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            ClearAllPlayers();

            Logger.Info("BoardRendererPlayerPieceTests.AddPlayerPiece_CreatesNewPiece", "We're here again", LogCategory.Core, this);
            Player testPlayer = CreateTestPlayer(0, "TestPlayer", Color.blue);

            yield return AddPlayerAndWait(testPlayer);
            
            Assert.AreEqual(1, boardRenderer.playerPieces.Count, "Should have one player piece");
            Assert.AreEqual(0, boardRenderer.playerPieces[0].playerId, "Player ID should match");
            Assert.AreEqual("TestPlayer", boardRenderer.playerPieces[0].name, "Player name should match");
        }
    }
}
```


## Important Notes

### `PlayMode` limitations

- Unity Coroutines are necessary to not fully lock the editor during testing cycles. Unfortunately this
means that most tasks that would be better to have in the PlayTestBase do not function correctly if they are
not run in the specific base class due to issues with how coroutines can call other functions.
- Each TestBase requires the following:
    - 2 calls in the test base setup:
        - SceneManager.sceneLoaded += OnSceneLoaded;
        - SceneManager.LoadScene("PlayTestScene", LoadSceneMode.Single);
    - implementation of the OnSceneLoaded(Scene scene, LoadSceneMode mode) function
        - setting the sceneLoaded boolean flag to True at the end of this method
    - Each test function must use the below function call:
        - yield return new WaitWhile(() => !sceneLoaded);



### Common Issues

- Forgetting to add the OnSceneLoaded function to the sceneLoaded event Que. 
- Forgetting to flag sceneLoaded to true
- Forgetting to WhileWhile(() => !sceneLoaded) (Becausse of courtines and async running, tests will run too early)

