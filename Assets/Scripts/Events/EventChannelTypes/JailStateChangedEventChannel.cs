using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Assets.Scripts.Events.EventDataStructures;

/// <summary>
/// this event channel passes jail state information when changed
/// <para>
/// to be owned by <c>JailManager</c> and subscribed to by any GameObjects that depend on
/// a player entering, leaving, or progressing through jail in order to update UI or game logic.
/// </para>
/// <para>
/// Payload: <c>JailStateChangedEvent</c>
/// </para>
/// </summary>

namespace Assets.Scripts.Events.EventChannelTypes
{
    [CreateAssetMenu(fileName = "JailStateChangedEventChannel", menuName = "Events/Jail State Changed Event Channel")]
    public class JailStateChangedEventChannel : ScriptableObject
    {
        public UnityAction<JailStateChangedEvent> OnEventRaised;


        public void RaiseEvent(JailStateChangedEvent jailEvent)
        {
            if (OnEventRaised != null)
            {
                OnEventRaised.Invoke(jailEvent);
                Debug.Log($"[Event Raised] Jail state changed for {jailEvent.player.GetPName()} (InJail: {jailEvent.inJail}, Turns: {jailEvent.jailTurns})");
            }
            else
            {
                Debug.LogWarning("No listeners registered for JailStateChangedEventChannel!");
            }
        }
    }
}
