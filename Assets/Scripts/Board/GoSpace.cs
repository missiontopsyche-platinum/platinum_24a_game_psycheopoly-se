using UnityEngine;

namespace PsycheOpoly.Board
{
    public class GoSpace : Space 
    {
        //Amount for passing the Go Space
        public const int Award = 200;

        public GoSpace(string name = "Go") : base(name) {}


        public override void OnLanded(Player player)
        {
            if (player == null)
            {
                //Catches any null player's 
                Logging.Logger.Warn("GoSpace.OnLanded", 
                    "Attempted to process a null player landing on Go space.", 
                    Logging.LogCategory.Gameplay,
                    this);
                return;
            }

            //Adding reward to player's money
            int newMoney = player.GetMoney() + Award;
            player.SetMoney(newMoney); //updated player status
            Logging.Logger.Info("GoSpace.OnLanded", 
                $"{player.GetPName()} landed on {Name} and received {Award}. Total: {newMoney}", 
                Logging.LogCategory.Gameplay,
                this);
        }
    }
}