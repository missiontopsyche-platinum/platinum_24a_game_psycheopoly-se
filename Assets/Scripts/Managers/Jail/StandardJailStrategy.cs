using UnityEngine;

namespace Assets.Scripts.Managers.Jail
{
    /// <summary>
    /// this implements the standard monopoly style jail behavior; implements JailStrategy interface
    /// </summary>
    public class StandardJailStrategy
    {
        // temporary constants until we have a configurable ruleset hook established
        private const int MAX_TURNS_IN_JAIL = 3;
        private const int JAIL_FEE = 100;

        public static void AttemptEscape(Player player, int dice1, int dice2)
        {
            int turns = player.GetJailTurns() + 1;

            player.SetJailTurns(turns);

            bool rolledDoubles = (dice1 == dice2);

            //this enters logic to evaluate if they rolled doubles OR if they reached the forced exit
            if (rolledDoubles)
            {
                Debug.Log($"{player.GetPName()} rolled doubles and escaped jail!");
                ReleasePlayer(player);
            } else if (turns >= MAX_TURNS_IN_JAIL)
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
        public static void PayFee(Player player)
        {
            if (player.GetMoney() >= JAIL_FEE)
            {
                player.SetMoney(player.GetMoney() - JAIL_FEE);
                Debug.Log($"{player.GetPName()} paid ${JAIL_FEE} to leave jail.");
                ReleasePlayer(player);
            }
            else
            {
                Debug.LogWarning($"{player.GetPName()} cannot afford the jail fee!");
                //TODO: bankruptcy comes here
            }
        }

        public static void UseGetOutOfJailFree(Player player)
        {
            if (player.GetChanceCardCount() > 0)
            {
                player.DecrementChanceCard();
                ReleasePlayer(player);
                Debug.Log($"{player.GetPName()} used a Chance Get-Out-Of-Jail-Free card.");
            }
            else if (player.GetCommunityCardCount() > 0)
            {
                player.DecrementCommunityCard();
                ReleasePlayer(player);
                Debug.Log($"{player.GetPName()} used a Community Chest Get-Out-Of-Jail-Free card.");
            }
            else
            {
                Debug.LogWarning($"{player.GetPName()} has no Get-Out-Of-Jail-Free cards!");
            }
        }

        private static void ForcedExit(Player player)
        {
            int money = player.GetMoney();
            int jailFee = JAIL_FEE;
            if (money >= jailFee)
            {
                player.SetMoney(money - jailFee);
                Debug.Log($"{player.GetPName()} was forced to pay ${jailFee} after 3 turns.");
            }
            else
            {
                //i'm not sure if we bankrupt here or what??? this is just a placeholder
                //TODO: trigger bankruptcy here
                Debug.LogWarning($"{player.GetPName()} cannot afford the forced jail fee!");
            }

            ReleasePlayer(player);
        }

        private static void ReleasePlayer(Player player)
        {
            player.SetInJail(false);
            player.SetJailTurns(0);
            Debug.Log($"{ player.GetPName()} is free from jail.");
        }
    }
}
