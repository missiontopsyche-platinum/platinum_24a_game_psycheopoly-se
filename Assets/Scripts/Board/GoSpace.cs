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
                Debug.LogWarning("GoSpace.OnLanded called with a null player");
                return;
            }

            //Adding reward to player's money
            int newMoney = player.GetMoney() + Award;
            player.SetMoney(newMoney); //updated player status
            Debug.Log($"{player.GetPName()} landed on {Name} and received {Award}. Total: {newMoney}");
        }
    }
}