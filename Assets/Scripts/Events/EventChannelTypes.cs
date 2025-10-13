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
/// Event channel that passes a GameStateChange struct, with the previous and current Game State.
/// </summary>
public class GameStateEventChannel : EventChannel<GameStateChange> { }