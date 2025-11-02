using UnityEngine;

/// <summary>
/// This is a channel dedicated to request the DiceManager asset to raise
/// RolledDiceEvent.
/// </summary>
[CreateAssetMenu(menuName = "Events/BooleanEventChannel")]
public class BooleanEventChannel : EventChannel<bool> { }
