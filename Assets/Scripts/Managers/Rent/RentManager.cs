using UnityEngine;
using Assets.Scripts.Managers.Rules;
using Events.EventDataStructures.UI;
using Assets.Scripts.Events.EventChannelTypes;

namespace Assets.Scripts.Managers.Rent
{
    [RequireComponent(typeof(OwnershipServiceAdapter))]
    [RequireComponent(typeof(CostModifierService))]
    public class RentManager : MonoBehaviour
    {
        [Header("Dependencies")]
        //[SerializeField] private EconomyAdapter economy;                     //money mover (placeholder) This might no longer exist. currently commented out until confirmation.
        [SerializeField] private OwnershipServiceAdapter ownership;          //ownership source of truth (adapter)
        [SerializeField] private CostModifierService rentModifiers;

        [Header("Events")]
        [SerializeField] private IntEventChannel rentComputedChannel;

        
        [Header("Debt Resolution Events")]
        [SerializeField] private UIActivationEventChannel uiActivationEventChannel;
        [SerializeField] private IntEventChannel bankruptPlayerEventChannel;

        public static RentManager Instance { get; private set; } // call .Instance."SomeMethod" to run

        private StandardRuleSet rules;
        public IOwnershipService Ownership => ownership;

        public void Start()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Awake()
        {
            ownership = GetComponent<OwnershipServiceAdapter>();
            rentModifiers = GetComponent<CostModifierService>();
            EnsureDependencies();
        }

        //transfer money for rent payment
        public void TryChargeRent(Player tenant, ITileRentInfo tile, int diceTotal)
        {
            if (tenant == null || tile == null)
                return;

            EnsureDependencies();

            if (rules == null)
            {
                Logging.Logger.Warn("RentManager", "Rules not initialized; rent charge skipped.");
                return;
            }

            Player owner = ownership.GetOwner(tile);
            if (owner == null || owner == tenant)
                return;

            int baseRent = RentCalculator.ComputeRent(tile, owner, diceTotal, ownership, rules);

            int rent = rentModifiers != null
                ? rentModifiers.ApplyAll(baseRent, tile, tenant, owner)
                : baseRent;

            Logging.Logger.Debug(
                "RentManager.TryChargeRent",
                $"BaseRent={baseRent} FinalRent={rent} Tenant={tenant?.GetId()} Owner={owner?.GetId()} Tile={tile?.Name}",
                Logging.LogCategory.Gameplay);

            rentComputedChannel?.RaiseEvent(rent);

            if (rent <= 0)
                return;

            TransferMoney(tenant, owner, rent);
        }

        private void EnsureDependencies()
        {
            if (!ownership)
                ownership = GetComponent<OwnershipServiceAdapter>();

            if (!rentModifiers)
                rentModifiers = GetComponent<CostModifierService>();

            if (rules == null)
                rules = StandardRuleSet.GetInstance();
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

        private bool TransferMoney(Player from, Player to, int amount)
        {
            if (from == null || to == null)
            {
                Logging.Logger.Error(
                    "RentManager.TransferMoney",
                    "Transfer failed: from/to player is null.",
                    Logging.LogCategory.Economy,
                    this);
                return false;
            }

            if (amount <= 0)
                return true;

            Player.FinancialStatus spendStatus = from.TrySpend(amount);

            switch (spendStatus)
            {
                case Player.FinancialStatus.Success:
                    to.AddMoney(amount);
                    return true;

                case Player.FinancialStatus.Bankrupt:
                    Logging.Logger.Warn(
                        "RentManager.TransferMoney",
                        $"Player {from.GetId()} is bankrupt and cannot cover rent of ${amount}.",
                        Logging.LogCategory.Economy,
                        this);

                    bankruptPlayerEventChannel?.RaiseEvent(from.GetId());
                    return false;

                case Player.FinancialStatus.MortgageRequired:
                    int debtAmount = Mathf.Max(0, amount - from.GetMoney());

                    Logging.Logger.Info(
                        "RentManager.TransferMoney",
                        $"Player {from.GetId()} entered debt resolution for ${debtAmount}.",
                        Logging.LogCategory.Economy,
                        this);

                    uiActivationEventChannel?.RaiseEvent(
                        new UIActivationEvent(
                            UIType.PropertyManagement,
                            new PropertyManagementActivationContext(from, true, debtAmount)));

                    return false;
            }

            return false;
        }
    }
}