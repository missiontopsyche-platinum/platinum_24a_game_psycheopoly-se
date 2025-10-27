using UnityEngine;

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
[CreateAssetMenu(menuName = "Events/DiceRolledEventChannel")]
public class DiceRolledEventChannel : EventChannel<DiceRolledEvent> { }