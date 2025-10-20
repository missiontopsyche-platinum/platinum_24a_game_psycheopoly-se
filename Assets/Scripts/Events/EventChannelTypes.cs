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
/// <para><i><b>
/// This is generic, mostly used for testing, until the more favored specialized Event Data payloads
/// have been developed.
/// </b></i></para>
/// <para>
/// Payload: <c>Player</c>
/// </para>
/// </summary>
public class PlayerEventChannel : EventChannel<Player> { }

/// <summary>
/// Event channel that passes Game State Change information.
/// <para>
/// To be owned by <c>GameManager</c> and subscribed to by any GameObjects that depend on
/// the game state changing to determine behaviour.
/// </para>
/// <para>
/// Payload: <c>GameStateChangedEvent</c>
/// </para>
/// </summary>
public class GameStateChangedEventChannel : EventChannel<GameStateChangedEvent> { }

/// <summary>
/// Event channel that sends information about Player movement
/// <para>
/// To be owned by <c>GameManager</c> and subscribed to by any GameObjects that depend on
/// the Player Position changing to determine behaviour.
/// </para>
/// <para>
/// Payload: <c>PlayerMovedEvent</c>
/// </para>
/// </summary>
public class PlayerMovedEventChannel : EventChannel<PlayerMovedEvent> { }

/// <summary>
/// Event channel that sends information about a turn starting.
/// <para>
/// To be owned by <c>GameManager</c> and subscribed to by any GameObjects that depend on
/// a notification that a turn has started to determine behaviour.
/// </para>
/// <para>
/// Payload: <c>TurnStartedEvent</c>
/// </para>
/// </summary>
public class TurnStartedEventChannel : EventChannel<TurnStartedEvent> { }

/// <summary>
/// Event channel that sends information about a dice roll.
/// <para>
/// To be owned by <c>GameManager</c> and subscribed to by any GameObjects that depend on
/// a notification that a dice was rolled to determine behaviour.
/// </para>
/// <para>
/// Payload: <c>DiceRolledEvent</c>
/// </para>
/// </summary>
public class DiceRolledEventChannel : EventChannel<DiceRolledEvent> { }

/// <summary>
/// Event channel that sends information on how to move a player
/// <para>
/// To be owned by <c>GameManager</c> and subscribed to by any GameObjects that depend on
/// a notification of how a player needs to move
/// </para>
/// <para>
/// Payload: <c>MovePlayerEventEvent</c>
/// </para>
/// </summary>
public class MovePlayerEventChannel : EventChannel<MovePlayerEvent> { }