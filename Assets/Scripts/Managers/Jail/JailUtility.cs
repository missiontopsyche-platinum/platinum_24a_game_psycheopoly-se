using UnityEngine;
using Logging;
using Logger = Logging.Logger;

namespace Assets.Scripts.Managers.Jail
{
    /// <summary>
    /// this implements the standard monopoly style jail behavior; implements JailStrategy interface
    /// </summary>
    public class JailUtility
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
                Logger.Info("JailUtility.AttemptEscape",
                    $"{player.GetPName()} rolled doubles and escaped jail!",
                    LogCategory.Gameplay);
                ReleasePlayer(player);
            } else if (turns >= MAX_TURNS_IN_JAIL)
            {
                Logger.Info("JailUtility.AttemptEscape",
                    $"{player.GetPName()} failed to roll doubles, but has served their sentence! Forced to pay & exit.",
                    LogCategory.Gameplay);
                ForcedExit(player);
            }
            else
            {
                Logger.Info("JailUtility.AttemptEscape",
                    $"{player.GetPName()} did not roll doubles. Turn {turns}/3 in jail.",
                    LogCategory.Gameplay);
            }
        }

        //first iteration is just going to charge the fee if the player can afford it
        public static void PayFee(Player player)
        {
            if (player.GetMoney() >= JAIL_FEE)
            {
                player.SetMoney(player.GetMoney() - JAIL_FEE);
                Logger.Info("JailUtility.PayFee",
                    $"{player.GetPName()} paid ${JAIL_FEE} to leave jail.",
                    LogCategory.Gameplay);
                ReleasePlayer(player);
            }
            else
            {
                Logger.Warn("JailUtility.PayFee",
                    $"{player.GetPName()} cannot afford the jail fee!",
                    LogCategory.Gameplay);
                //TODO: bankruptcy comes here
            }
        }

        public static void UseGetOutOfJailFree(Player player)
        {
            if (player.GetChanceCardCount() > 0)
            {
                player.DecrementChanceCard();
                ReleasePlayer(player);
                Logger.Info("JailUtility.UseGetOutOfJailFree",
                    $"{player.GetPName()} used a Chance Get-Out-Of-Jail-Free card.",
                    LogCategory.Gameplay);
            }
            else if (player.GetCommunityCardCount() > 0)
            {
                player.DecrementCommunityCard();
                ReleasePlayer(player);
                Logger.Info("JailUtility.UseGetOutOfJailFree",
                    $"{player.GetPName()} used a Community Chest Get-Out-Of-Jail-Free card.",
                    LogCategory.Gameplay);
            }
            else
            {
                Logger.Warn("JailUtility.UseGetOutOfJailFree",
                    $"{player.GetPName()} has no Get-Out-Of-Jail-Free cards!",
                    LogCategory.Gameplay);
            }
        }

        private static void ForcedExit(Player player)
        {
            int money = player.GetMoney();
            if (money >= JAIL_FEE)
            {
                player.SetMoney(money - JAIL_FEE);
                Logger.Info("JailUtility.ForcedExit",
                    $"{player.GetPName()} was forced to pay ${JAIL_FEE} after 3 turns.",
                    LogCategory.Gameplay);
            }
            else
            {
                //i'm not sure if we bankrupt here or what??? this is just a placeholder
                //TODO: trigger bankruptcy here
                Logger.Warn("JailUtility.ForcedExit",
                    $"{player.GetPName()} cannot afford the forced jail fee!",
                    LogCategory.Gameplay);
            }

            ReleasePlayer(player);
        }

        private static void ReleasePlayer(Player player)
        {
            player.SetInJail(false);
            player.SetJailTurns(0);
            Logger.Info("JailUtility.ReleasePlayer",
                $"{player.GetPName()} is free from jail.",
                LogCategory.Gameplay);
        }
    }
}
