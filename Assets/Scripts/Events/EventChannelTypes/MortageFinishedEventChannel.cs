using Events.EventDataStructures.UI;
using UnityEngine;

namespace Assets.Scripts.Events.EventChannelTypes
{
    [CreateAssetMenu(menuName = "Events/Mortage Finished Event Channel")]
    public class MortageFinishedEventChannel : EventChannel<MortageFinishedEvent> { }
}