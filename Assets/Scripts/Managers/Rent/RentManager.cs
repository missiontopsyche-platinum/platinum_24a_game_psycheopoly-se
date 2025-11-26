using UnityEngine;
using Assets.Scripts.Managers.Rules;


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
        [SerializeField] private RulesManager rulesManager;
        [SerializeField] private RentModifierService rentModifiers;


        private IRuleSet rules;
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

            //replacing basic rent to use modifiers
            int baseRent = strategy.ComputeRent(tile, owner, diceTotal, ownership, rules);
            int rent = rentModifiers != null ? rentModifiers.ApplyAll(baseRent, tile, tenant, owner) : baseRent;

            Logging.Logger.Debug("RentManager.TryChargeRent",
                $"BaseRent={baseRent} FinalRent={rent} Tenant={tenant?.GetId()} Owner={owner?.GetId()} Tile={tile?.Name}",
                Logging.LogCategory.Gameplay);

            if (rent <= 0) return;

            bool ok = economy.Transfer(tenant, owner, rent);
        }

        //Make sure serialized dependencies are not null
        private void EnsureDependencies()
        {
            if (!rentModifiers)
                rentModifiers = GetComponent<RentModifierService>() ?? gameObject.AddComponent<RentModifierService>();

            if (!economy)
                economy = GetComponent<EconomyAdapter>() ?? gameObject.AddComponent<EconomyAdapter>();

            if (!ownership)
                ownership = GetComponent<OwnershipServiceAdapter>() ?? gameObject.AddComponent<OwnershipServiceAdapter>();

            if (!rulesManager)
                rulesManager = FindObjectOfType<RulesManager>();

            if (rules == null && rulesManager != null)
                rules = rulesManager.GetRuleSet();
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
}
