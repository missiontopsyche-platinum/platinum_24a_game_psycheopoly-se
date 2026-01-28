using Events.EventDataStructures.UI;
using UnityEngine;

namespace Assets.Scripts.Events.EventChannelTypes
{
    [CreateAssetMenu(menuName = "Events/UI Action Event Channel")]
    public class UIActionEventChannel : EventChannel<UIActionEvent> { }
}