using System;
using UnityEngine;

/// <summary>
/// Example EventChannel type that passes Strings as event parameters.
/// </summary>
[CreateAssetMenu(menuName = "Events/String Event Channel")]
public class StringEventChannel : EventChannel<String> {}

/// <summary>
/// Example EventChannel type that passes `int` as event parameters.
/// </summary>
[CreateAssetMenu(menuName = "Events/Int Event Channel")]
public class IntEventChannel : EventChannel<int> {}

/// <summary>
/// Event channel that passes Player data objects.
/// </summary>
public class PlayerEventChannel : EventChannel<Player> { }

/// <summary>
/// Event channel that passes Game State Change information.
/// </summary>
public class GameStateChangedEventChannel : EventChannel<GameStateChangedEvent> { }

/// <summary>
/// Event channel that sends information about Player movement
/// </summary>
public class PlayerMovedEventChannel : EventChannel<PlayerMovedEvent> { }

/// <summary>
/// Event channel that sends information about a turn starting.
/// </summary>
public class TurnStartedEventChannel : EventChannel<TurnStartedEvent> { }