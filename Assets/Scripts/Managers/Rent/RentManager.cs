using UnityEngine;
using Assets.Scripts.Managers.Rent;


namespace Assets.Scripts.Managers.Rent
{
    ///Gets tile and owner, asks the strategy for the rent, and moves money.
    ///Self-wires Economy/Ownership/Rules in Awake() 
    ///so it works in EditMode tests where Awake() may not fire.
    public class RentManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerManager playerManager;                //not used by rent calc yet
        [SerializeField] private EconomyAdapter economy;                     //money mover (placeholder)
        [SerializeField] private OwnershipServiceAdapter ownership;          //ownership source of truth (adapter)
        [SerializeField] private RuleSet rules = new RuleSet();              //rule constants

        private IRentStrategy strategy = new StandardRentStrategy();
        public IOwnershipService Ownership => ownership;

        private void Awake()
        {
            EnsureDependencies();
        }

        //Compute and transfer rent when a tenant lands on a tile.
        //tentant - The landing player
        //tile - Tile adapter exposing rent-relevant data
        //dicetotal - Needed for utilities
        public void TryChargeRent(Player tenant, ITileRentInfo tile, int diceTotal)
        {
            if (tenant == null || tile == null) return;

            EnsureDependencies();

            var owner = ownership.GetOwner(tile);
            if (owner == null || owner == tenant) return;

            int rent = strategy.ComputeRent(tile, owner, diceTotal, ownership, rules);
            if (rent <= 0) return;

            bool ok = economy.Transfer(tenant, owner, rent);
        }

        //Make sure serialized dependencies are not null
        private void EnsureDependencies()
        {
            if (!economy)
                economy = GetComponent<EconomyAdapter>() ?? gameObject.AddComponent<EconomyAdapter>();

            if (!ownership)
                ownership = GetComponent<OwnershipServiceAdapter>() ?? gameObject.AddComponent<OwnershipServiceAdapter>();

            if (rules == null)
                rules = new RuleSet();
        }

        //helpers
        public void SetOwner(ITileRentInfo tile, Player owner)
        {
            EnsureDependencies();
            ownership.SetOwner(tile, owner);
        }

        public int CountOwnedInGroup(Player owner, ColorGroup group)
        {
            EnsureDependencies();
            return ownership.CountOwnedInGroup(owner, group);
        }

        public int CountRailroadsOwned(Player owner)
        {
            EnsureDependencies();
            return ownership.CountRailroadsOwned(owner);
        }
    }

    //Money mover. Replace with your real EconomyManager later.
    public class EconomyAdapter : MonoBehaviour
    {
        public bool Transfer(Player from, Player to, int amount)
        {
            if (amount <= 0) return true;

            int have = from.GetMoney();
            if (have < amount)
            {
                //placeholder trigger bankruptcy flow, for now pay what you can
                amount = have;
            }

            from.SetMoney(have - amount);
            to.SetMoney(to.GetMoney() + amount);
            return true;
        }
    }



    //Rule constants for standard Monopoly rent. can be converted to ScriptableObject
    [System.Serializable]
    public class RuleSet : IRuleSet
    {
        [Tooltip("Base rent for 1 owned railroad (25, 50, 100, 200 scaling).")]
        public int RailroadBase = 25;

        [Tooltip("Utility multiplier when owner has a single utility.")]
        public int UtilitySingle = 4;

        [Tooltip("Utility multiplier when owner has both utilities.")]
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
