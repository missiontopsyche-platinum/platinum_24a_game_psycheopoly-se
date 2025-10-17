# Testing Infrastructure

## Overview

This folder contains the `EditMode` tests for the project. Tests are organized by
component with base classes that handle common `SetUp()`/`TearDown()` to reduce
boilerplate redundancy and increase maintainability.

## Folder Structure

```
Tests/EditMode/
├── ManagerTestBase.cs              # Shared utilities for all manager tests
├── README.md                       # This file
├── _TestFrameworkVerification.cs   # Sanity checks that test runner works
├── BoardManagerTests/
│   ├── BoardManagerTestBase.cs     # Setup/teardown for BoardManager
│   └── [specific test files]
├── EventsTests/
├── GameManagerTests/
│   ├── GameManagerTestBase.cs      # Setup/teardown for GameManager
│   └── [specific test files]
├── PlayerManagerTests/
│   ├── PlayerManagerTestBase.cs    # Setup/teardown for PlayerManager
│   └── [specific test files]
├── PlayerTests/
└── SpacesTests/
```

## Base Class Pattern

### ManagerTestBase

All manager test bases inherit from `ManagerTestBase`, which provides:

- `CreateChannel<T>()` : Creates and tracks EventChannel instances
- `DestroyTestObjects(params UnityEngine.Object[])` : Cleanup utility

### Example

```csharp
public class GameManagerTestBase : ManagerTestBase
{
    protected GameObject gameManagerObject;
    protected GameManager gameManager;
    
    [SetUp]
    public virtual void SetUp()
    {
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
        
        // Automatically tracked for cleanup
        gameManager.someChannel = CreateChannel<SomeEventChannel>();
    }
    
    [TearDown]
    public virtual void TearDown()
    {
        DestroyTestObjects(gameManagerObject);
        // Channels automatically cleaned up
    }
}
```

## Component-Specific Base Classes

Each `MonoBehaviour` has its own test base (`GameManagerTestBase`,
`PlayerManagerTestBase`, etc) that:
- Inherits from `ManagerTestBase`
- Creates the `GameObject` and component
- Instantiates all required Event Channels
- Provides `protected` fields for test access

## Writing New Tests

### For a New Test Class

```csharp
using NUnit.Framework;
using Tests.EditMode.GameManagerTests;

// replace with whatever namespace this should be in
namespace Tests.EditMode.GameManagerTests 
{
    public class GameManagerStartGameTests : GameManagerTestBase
    {
        [Test]
        public void StartGame_WithFourPlayers_InitializesCorrectly()
        {
            // gameManager and all channels are already set up
            gameManager.StartGame(4);
            
            Assert.AreEqual(4, gameManager.PlayerCount);
        }
    }
}
```

All test classes inheriting from this base class will now have the new channel
set up automatically. This is the biggest benefit of this class pattern: add once
to update everywhere.

### For a New `MonoBehaviour`

1. Create `YourComponentTestBase : ManagerTestBase`
2. Implement `SetUp()` to create `GameObject` and component
3. Use `CreateChannel<T>()` for all `EventChannel` hooks
4. Implement `TearDown()` to call `DestroyTestObjects(gameObject)`
5. Create specific test classes that inherit from your base

### Updating an Existing Base Class

When you add a new dependency (like an `EventChannel`) to a `MonoBehaviour`:

```csharp
public class GameManagerTestBase : ManagerTestBase
{
    protected GameObject gameManagerObject;
    protected GameManager gameManager;
    
    [SetUp]
    public virtual void SetUp()
    {
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
        
        gameManager.existingChannel = CreateChannel<ExistingEventChannel>();
        
        // Add the new channel here, all child tests automatically get it
        gameManager.newChannel = CreateChannel<NewEventChannel>();
    }
    
    [TearDown]
    public virtual void TearDown()
    {
        DestroyTestObjects(gameManagerObject);
        // No changes needed, channels are automatically cleaned up
    }
}
```

## Important Notes

### `EditMode` limitations

- Unity Lifecycle methods don't autofire: `Awake()`, `Start()`, `OnEnable()`
  won't run automatically
- If the component subscribes to events in one of these methods, you must
  manually call the subscription in `SetUp()`:
  `manager.eventChannel.Subscribe(manager.EvokeMethod);`
- EventChannels must be explicitly instantiated with `CreateChannel<T>()`

### EventChannelPattern

```csharp
// Always create channels, even if not testing them directly
gameManager.someChannel = CreateChannel<SomeEventChannel>();

// Access channels directly from the manager in tests
gameManager.someChannel.Subscribe(data => { /* test logic */ });
```

### Common Issues

- Forgetting to call `base.SetUp()` or `base.TearDown()` when overriding base
  methods
- Using `[SetUp]` instead of `[TearDown]` tags
- Not creating `EventChannel` instances (causes `NullReferenceException`)
- Expecting `Awake()` or `Start()` to run automatically in `EditMode` tests