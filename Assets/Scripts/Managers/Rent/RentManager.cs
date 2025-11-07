using UnityEngine;
using Logging; 

namespace Assets.Scripts.Managers.Rent
{
    ///Gets tile and owner then asks strategy for rent 
    ///then moves money.  Also subscribes to landing trigger 
    public class RentManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerManager PlayerManager;
        [SerializeField] private EconomyAdapter economy; //money mover for now
        [SerializeField] private OwnershipServiceAdapter ownership;
        [SerializeField] private RuleSet rules = new();

        private IRentStrategy strategy = new StandardRentStrategy();

        ///Compute and transfer rent for a landing. 
        ///tenant - player who landed
        ///tile - tile info adapter for strategy
        ///diceTotal - needed for utilities 
        public void TryChargeRent(Player tenant, ITileRentInfo tile, int diceTotal)
        {
            if (tenant == null || tile == null) return;

            var owner = ownership.GetOwner(tile);
            if(owner == null || owner == tenant) return;

            int rent = strategy.ComputeRent(tile, owner, diceTotal, ownership, rules);
            if (rent <= 0) return; 

            bool ok = economy.Transfer(tenant, owner, rent);
        }
    }

    //Placeholder until final version of economy adapter is created
    public class EconomyAdapter : MonoBehaviour
    {
        public bool Transfer(Player from, Player to, int amount)
        {
            if (amount <= 0) return true;
            int have = from.GetMoney();
            if (have < amount) amount = have; //Trigger Bankruptcy here
            from.SetMoney(have - amount);
            to.SetMoney(to.GetMoney() + amount);
            return true; 
        }
    }

    //Rule constants 
    [System.Serializable]
    public class RuleSet : IRuleSet{
        public int RailroadBase = 25;
        public int UtilitySingle = 4;
        public int UtilityBoth = 10;

        public int RailroadBaseRent() => RailroadBase;
        public int UtilityRentSingleMult() => UtilitySingle;
        public int UtilityRentBothMult() => UtilityBoth;

        //Default Monopoly set sizes
        public int StreetsInGroup(ColorGroup g) =>
            g switch
            {
                ColorGroup.Brown or ColorGroup.DarkBlue => 2,
                ColorGroup.LightBlue or ColorGroup.Pink or ColorGroup.Orange or ColorGroup.Red or ColorGroup.Yellow or ColorGroup.Green => 3,
                _ => 0
            };
    }

}