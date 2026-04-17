using Events.EventDataStructures.UI;
using UnityEngine;

namespace Assets.Scripts.Events.EventChannelTypes
{
    [CreateAssetMenu(menuName = "Events/Mortgage Finished Event Channel")]
    public class MortgageFinishedEventChannel : EventChannel<MortgageFinishedEvent> { }
}