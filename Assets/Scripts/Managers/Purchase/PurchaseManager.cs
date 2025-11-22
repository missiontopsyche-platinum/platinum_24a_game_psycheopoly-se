using UnityEngine;
using Assets.Scripts.Managers.Rent;

namespace Assets.Scripts.Managers.Purchase
{
    //Gets tile and buyer, asks the strategy what to do, and executes the purchase
    //Self wires Ownership/Rules in Awake() similar to RentManager
    public class PurchaseManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private OwnershipServiceAdapter ownership;
        [SerializeField] private RuleSet rules = new RuleSet();

        private IPurchaseStrategy strategy = new StandardPurchaseStrategy();

        private void Awake()
        {
            EnsureDependencies();
        }

        //Call when player lands on tile that might be for sale
        public void TryHandlePurchase(Player buyer, ITileRentInfo tile)
        {
            if (buyer == null || tile == null)
                return;

            EnsureDependencies();

            PurchaseDecision decision =
                strategy.GetPurchaseDecision(tile, buyer, ownership, rules);

            if (decision.Flow == PurchaseFlow.None || decision.Price <= 0)
                return;

            HandleDecision(buyer, tile, decision);
        }

        //transfers money and assigns ownership
        private void ExecutePurchase(Player buyer, ITileRentInfo tile, int price)
        {
            int have = buyer.GetMoney();
            if (have < price)
            {
                //Should not happen if strategy did its job but guards anyway.
                price = have;
            }

            buyer.SetMoney(have - price);
            ownership.SetOwner(tile, buyer);
        }

        //Make sure serialized dependencies are not null
        private void EnsureDependencies()
        {
            if (!ownership)
                ownership = GetComponent<OwnershipServiceAdapter>() ??
                            gameObject.AddComponent<OwnershipServiceAdapter>();

            if (rules == null)
                rules = new RuleSet();
        }

        private void HandleDecision(Player buyer, ITileRentInfo tile, PurchaseDecision decision)
        {
            switch (decision.Flow)
            {
                case PurchaseFlow.AutoBuy:
                    ExecutePurchase(buyer, tile, decision.Price);
                    break;

                case PurchaseFlow.OfferToPlayer:
                    //TO DO: Hook to a real popup.
                    //For now use auto accept to keep the flow working
                    ExecutePurchase(buyer, tile, decision.Price);
                break;

                case PurchaseFlow.HookAction:
                    //Placeholder for custom flows like auction manager
                    //action hook as in task 447
                    break;

                case PurchaseFlow.None:
                default:
                    break;
            }
        }
    }
}
