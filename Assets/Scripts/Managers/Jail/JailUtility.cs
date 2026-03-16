using Logging;
using Logger = Logging.Logger;

namespace Assets.Scripts.Managers.Jail
{
    public static class JailUtility
    {
        // return enums
        public enum EscapeAttemptResult { Escaped, Failed, ForcedExitPaid, ForcedExitBankrupt }
        public enum FeePaymentResult { Paid, Bankrupt }
        public enum CardUseResult { Success, NoCardAvailable }
        
        // temporary constants until we have a configurable ruleset hook established
        private const int MAX_TURNS_IN_JAIL = 3;
        private const int JAIL_FEE = 100;

        public static EscapeAttemptResult AttemptEscape(Player player, int dice1, int dice2)
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
                return EscapeAttemptResult.Escaped;
            }
            if (turns >= MAX_TURNS_IN_JAIL)
            {
                Logger.Info("JailUtility.AttemptEscape",
                    $"{player.GetPName()} failed to roll doubles, but has served their sentence! Forced to pay & exit.",
                    LogCategory.Gameplay);
                return ForcedExit(player);
            }
            
            Logger.Info("JailUtility.AttemptEscape",
                $"{player.GetPName()} did not roll doubles. Turn {turns}/3 in jail.",
                LogCategory.Gameplay);
            return EscapeAttemptResult.Failed;
        }
        
        public static FeePaymentResult PayFee(Player player)
        {
            FeePaymentResult result = ChargeJailFee(player);
            ReleasePlayer(player);
            
            if (result == FeePaymentResult.Paid)
            {
                Logger.Info("JailUtility.PayFee",
                    $"{player.GetPName()} paid ${JAIL_FEE} to leave jail.",
                    LogCategory.Gameplay);
            }
            else
            {
                Logger.Warn("JailUtility.PayFee",
                    $"{player.GetPName()} cannot afford the jail fee!",
                    LogCategory.Gameplay);
            }

            return result;
        }

        public static CardUseResult UseGetOutOfJailFree(Player player)
        {
            if (player.GetChanceCardCount() > 0)
            {
                player.DecrementChanceCard();
                ReleasePlayer(player);
                
                Logger.Info("JailUtility.UseGetOutOfJailFree",
                    $"{player.GetPName()} used a Chance Get-Out-Of-Jail-Free card.",
                    LogCategory.Gameplay);

                return CardUseResult.Success;
            }
            if (player.GetCommunityCardCount() > 0)
            {
                player.DecrementCommunityCard();
                ReleasePlayer(player);
                Logger.Info("JailUtility.UseGetOutOfJailFree",
                    $"{player.GetPName()} used a Community Chest Get-Out-Of-Jail-Free card.",
                    LogCategory.Gameplay);
                
                return CardUseResult.Success;
            }
            
            Logger.Warn("JailUtility.UseGetOutOfJailFree",
                $"{player.GetPName()} has no Get-Out-Of-Jail-Free cards!",
                LogCategory.Gameplay);

            return CardUseResult.NoCardAvailable;
        }

        private static EscapeAttemptResult ForcedExit(Player player)
        {
            FeePaymentResult result = ChargeJailFee(player);
            ReleasePlayer(player);
            if (result == FeePaymentResult.Paid)
            {
                Logger.Info("JailUtility.ForcedExit",
                    $"{player.GetPName()} was forced to pay ${JAIL_FEE} after 3 turns.",
                    LogCategory.Gameplay);
                return EscapeAttemptResult.ForcedExitPaid;
            }
            
            Logger.Warn("JailUtility.ForcedExit",
                $"{player.GetPName()} cannot afford the forced jail fee!",
                LogCategory.Gameplay);
            return EscapeAttemptResult.ForcedExitBankrupt;
        }

        private static FeePaymentResult ChargeJailFee(Player player)
        {
            FeePaymentResult result = player.GetMoney() >= JAIL_FEE 
                ? FeePaymentResult.Paid 
                : FeePaymentResult.Bankrupt;

            player.SetMoney(player.GetMoney() - JAIL_FEE);
            
            return result;
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
