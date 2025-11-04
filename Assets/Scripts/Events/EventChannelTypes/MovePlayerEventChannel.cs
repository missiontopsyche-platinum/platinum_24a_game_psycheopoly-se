using UnityEngine;

/// <summary>
/// Event channel that sends information about how to move a player
/// <para>
/// To be owned by <c>GameManager</c> and subscribed to by any GameObjects that depend on
/// a notification that player needs to move
/// </para>
/// <para>
/// Payload: <c>MovePlayerPayload</c>
/// </para>
/// </summary>
[CreateAssetMenu(menuName = "Events/MovePlayerEventChannel")]
public class MovePlayerEventChannel : EventChannel<MovePlayerEvent> { }