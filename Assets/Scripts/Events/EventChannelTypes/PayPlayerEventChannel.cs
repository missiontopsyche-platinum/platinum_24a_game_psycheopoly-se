using Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// <para>
/// Event channel that sends information about how to pay a player
/// </para>
/// <para>
/// Payload: <c>PayPlayerPayload</c>
/// </para>
/// </summary>
[CreateAssetMenu(menuName = "Events/PayPlayerEventChannel")]
public class PayPlayerEventChannel : EventChannel<PayPlayerEvent> { }