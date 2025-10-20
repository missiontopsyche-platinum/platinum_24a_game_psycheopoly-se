using UnityEngine;

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
[CreateAssetMenu(menuName = "Events/Player Event Channel")]
public class PlayerEventChannel : EventChannel<Player> { }