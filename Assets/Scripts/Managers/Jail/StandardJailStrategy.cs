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
    /// 
    //TODO: when we build out JailManager/TurnManager, we will need to implement the ability to roll after
    //escaping jail.
    public class StandardJailStrategy : IJailStrategy
    {
        private const int JailFee = 100;
        private const int MaxTurnsInJail = 3;

        public void AttemptEscape(Player player, int dice1, int dice2)
        {
            int turns = player.GetJailTurns() + 1;

            player.SetJailTurns(turns);

            bool rolledDoubles = (dice1 == dice2);

            //this enters logic to evaluate if they rolled doubles OR if they reached the forced exit
            if (rolledDoubles)
            {
                Debug.Log($"{player.GetPName()} rolled doubles and escaped jail!");
                ReleasePlayer(player);
            } else if (turns >= MaxTurnsInJail)
            {
                Debug.Log($"{player.GetPName()} failed to roll doubles, but has served their sentence! Forced to pay & exit.");
                ForcedExit(player);
            }
            else
            {
                Debug.Log($"{player.GetPName()} did not roll doubles. Turn {turns}/3 in jail.");
            }
        }

        //first iteration is just going to charge the fee if the player can afford it
        public void PayFee(Player player)
        {
            if (player.GetMoney() >= JailFee)
            {
                player.SetMoney(player.GetMoney() - JailFee);
                Debug.Log($"{player.GetPName()} paid ${JailFee} to leave jail.");
                ReleasePlayer(player);
            }
            else
            {
                Debug.LogWarning($"{player.GetPName()} cannot afford the jail fee!");
            }
        }

        public void UseGetOutOfJailFree(Player player)
        {
            //TODO: in T287, we'll need to implement these cards. Commented out now to allow the commit. ------------------------------------------------------------
            //but this logic is sound.
            //if (player.GetOutOfJailFree_Chance > 0)
            //{
            //    player.GetOutOfJailFree_Chance--;
            //    Debug.Log($"{player.GetPName()} used a Chance Get-Out-Of-Jail-Free card.");
            //    ReleasePlayer(player);
            //}
            //else if (player.GetOutOfJailFree_Community > 0)
            //{
            //    player.GetOutOfJailFree_Community--;
            //    Debug.Log($"{player.GetPName()} used a Community Chest Get-Out-Of-Jail-Free card.");
            //    ReleasePlayer(player);
            //}
            //else
            //{
            //    Debug.LogWarning($"{player.GetPName()} has no Get-Out-Of-Jail-Free cards!");
            //}
        }

        public void ForcedExit(Player player)
        {
            int money = player.GetMoney();
            if (money >= JailFee)
            {
                player.SetMoney(money - JailFee);
                Debug.Log($"{player.GetPName()} was forced to pay ${JailFee} after 3 turns.");
            }
            else
            {
                //i'm not sure if we bankrupt here or what??? this is just a placeholder
                //TODO: trigger bankruptcy here
                Debug.LogWarning($"{player.GetPName()} cannot afford the forced jail fee!");
            }

            ReleasePlayer(player);
        }

        private void ReleasePlayer(Player player)
        {
            player.SetInJail(false);
            player.SetJailTurns(0);
            Debug.Log($"{ player.GetPName()} is free from jail.");
        }
    }
}
