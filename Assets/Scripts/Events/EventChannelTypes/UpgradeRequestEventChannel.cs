using UnityEngine;
using Events.EventDataStructures;

[CreateAssetMenu(menuName = "Events/Upgrade Request Event Channel")]
public class UpgradeRequestEventChannel : EventChannel<UpgradeRequestEvent> { }
