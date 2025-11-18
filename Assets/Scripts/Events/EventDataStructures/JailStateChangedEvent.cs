using System;
using System.Collections.Generic;

/// <summary>
/// this event channel just a data container for the event of chaning jail status changed 
/// <para>
/// contains affected <c>Player</c> reference. whether the player is actively in jail,
/// and the number of turns they have spent there.
/// </para>
/// </summary>
namespace Assets.Scripts.Events.EventDataStructures
{
    [System.Serializable]
    public class JailStateChangedEvent
    {
        public Player player;
        public bool inJail;
        public int jailTurns;

        public JailStateChangedEvent(Player player, bool inJail, int jailTurns)
        {
            this.player = player;
            this.inJail = inJail;
            this.jailTurns = jailTurns;
        }
    }
}
