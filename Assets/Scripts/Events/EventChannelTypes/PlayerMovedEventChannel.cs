using UnityEngine;

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
[CreateAssetMenu(menuName = "Events/Player Moved Event Channel")]
public class PlayerMovedEventChannel : EventChannel<PlayerMovedEvent> { }
