using Events.EventDataStructures.UI;
using UnityEngine;

namespace Assets.Scripts.Events.EventChannelTypes
{
    [CreateAssetMenu(menuName = "Events/UI Activation Event Channel")]
    public class UIActivationEventChannel : EventChannel<UIActivationEvent> { }
}