using UnityEngine;

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
[CreateAssetMenu(menuName = "Events/Turn Started Event Channel")]
public class TurnStartedEventChannel : EventChannel<TurnStartedEvent> { }