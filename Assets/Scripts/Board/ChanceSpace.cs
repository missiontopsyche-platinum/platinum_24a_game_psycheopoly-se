using UnityEngine;

namespace  PsycheOpoly.Board
{
    public class ChanceSpace : Space 
    {
        public ChanceSpace(string name = "Chance") : base(name) {}

        public override void OnLanded(Player player)
        {
            //Sprint 1 placeholder, draw card logic can be added here
        }
    }
}