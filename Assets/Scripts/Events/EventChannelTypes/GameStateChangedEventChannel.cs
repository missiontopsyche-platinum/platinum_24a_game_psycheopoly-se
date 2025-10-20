using UnityEngine;

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
[CreateAssetMenu(menuName = "Events/Game State Changed Event Channel")]
public class GameStateChangedEventChannel : EventChannel<GameStateChangedEvent> { }

