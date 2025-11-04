using Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// Event channel that sends space information for display on hover.
/// <para>
/// To be listened to by OnHoverUI, and fired by SpaceData when SpaceRenderer is hovered.
/// </para>
/// <para>
/// Payload: <c>SpaceHoverEvent</c>
/// </para>
/// </summary>
[CreateAssetMenu(menuName = "Events/Space Hover Event Channel")]
public class SpaceHoverEventChannel : EventChannel<SpaceHoverEvent> { }
