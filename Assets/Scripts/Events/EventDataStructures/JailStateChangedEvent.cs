using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// this event channel just a data container for the event of chaning jail status changed 
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
