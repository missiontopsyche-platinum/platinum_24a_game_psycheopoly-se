using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(
    fileName = "UpgradeRequestEventChannel",
    menuName = "Events/Upgrade Request Event Channel")]
public class UpgradeRequestEventChannel : EventChannel<UpgradeRequestEvent>
{
}