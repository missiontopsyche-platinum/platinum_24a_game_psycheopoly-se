using UnityEngine;

namespace Assets.Scripts.Events.EventChannelTypes
{
    [CreateAssetMenu(
        fileName = "TurnPhaseChangedEventChannel",
        menuName = "Events/Turn Phase Changed Event Channel")]
    public class TurnPhaseChangedEventChannel : EventChannel<TurnPhaseChangedEvent>
    { }
}
