# Event Channel Documentation

## Overview

`EventChannel<T>` is a generic ScriptableObject-based event system that enables
decoupled communication between components. The `T` parameter specifies the type
of data passed to listeners when events are raised.

## Creating Event Channels

### 1. Define a Concrete Type

Event channels require concrete implementations to work with Unity's asset system.
These are defined in `EventChannelTypes.cs`:

```csharp
[Create Asset Menu(menuName = "Events/Int Event Channel")]
public class IntEventChannel : EventChannel<int> { }
```
### 2. Create an Asset

Right-click in the Project window. Go to `Create/Events/[Your Event Type]`.

### 3. Reference in Components

```csharp
public class ExampleComponent : Monobehaviour
{
    [SerializeField] private IntEventChannel scoreChangedChannel;
    
    private void OnEnable()
    {
        scoreChangedChannel.Subscribe(OnScoreChanged);
    }
    
    private void OnDisable()
    {
        scoreChangedChannel.Unsubscribe(OnScoreChanged);
    }
    
    private void OnScoreChanged(int newScore)
    {
        Debug.Log($"Score changed to: {newScore}");
}
```

## API Reference

### Methods

- `Subscribe<Action<T> listener)` : Registers a listener. Prevents duplicate
  subscriptions. `Action<T>` is a method name to be triggered.
- `Unsubscribe<Action<T> listener)` : Removes a listener.
- `RaiseEvent(T data)` : Invokes all subscribed listeners method with the 
  supplied data.
- `ClearAllListeners()` : Removes all listeners (useful for cleanup)

### Null Safety

- Attempting to subscribe a `null` listener will log a warning and be ignored.
- The `?.Invoke()` pattern ensures that listeners are safely invoked, even 
  if `null`.

### Examples

See `EventChannelTests.cs` for unit test examples demonstrating all core
functionality.

## Best Practices

- Always unsubscribe in `OnDisable()` or `OnDestroy()` to prevent memory leaks
- Use `[SerializeField]` to expose channels in the Inspector for easy wiring
- Create specfic event channels for different game events (`PlayerMovedChannel`,
  `TurnStartedChannel`, etc.)
- Name your channel assets descriptively ("PlayerMovedChannel", not "Event1")

### Why Use This?

- Loose coupling: Components don't need references to each other.
- Reusability: Same channel can be used by multiple systems.
- Debuggability: Channel assets are visible in teh project and can be inspected
  at runtime.
- Testability: Easy to create channel instances in unit tests.