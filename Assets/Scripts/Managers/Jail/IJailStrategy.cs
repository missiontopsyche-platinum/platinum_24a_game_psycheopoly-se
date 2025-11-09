using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Managers.Jail
{
    public interface IJailStrategy
    {
        //this is for the rollng doubles option, players do this every turn
        void AttemptEscape(Player player, int dice1, int dice2);

        //this will be done to escape jail early or can also be used when forced to exit on 3rd chance
        void PayFee(Player player);

        //this is the get out of ajial free card
        void UseGetOutOfJailFree(Player player);

        //and this will only occur once the player is on their third round in jail
        void ForcedExit(Player player);
    }
}
