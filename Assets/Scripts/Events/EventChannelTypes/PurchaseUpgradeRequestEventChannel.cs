using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Purchase Upgrade Request Event Channel")]
public class PurchaseUpgradeRequestEventChannel : EventChannel<PurchaseOwnableRequestEvent>
{ }
