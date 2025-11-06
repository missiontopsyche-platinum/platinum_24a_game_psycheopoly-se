using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Managers.Jail
{
    /// <summary>
    /// this implements the standard monopoly style jail behavior; implements JailStrategy interface
    /// </summary>
    public class StandardJailStrategy : JailStrategy
    {
        private const int JailFee = 100;
        private const int MaxTurnsInJail = 3;

        public void AttemptEscape(Player player, int dice1, int dice2)
        {
        }

        public void PayFee(Player player)
        {
        }

        public void UseGetOutOfJailFree(Player player)
        {
        }

        public void ForcedExit(Player player)
        {
        }

        private void ReleasePlayer(Player player)
        {
        }
    }
}
