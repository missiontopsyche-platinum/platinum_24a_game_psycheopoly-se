using Events.EventDataStructures;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Purchase Ownable Request Event Channel")]
public class PurchaseOwnableRequestEventChannel : EventChannel<PurchaseOwnableRequestEvent>
{ }
