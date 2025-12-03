using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;

namespace Assets.Scripts.Managers.Purchase
{
    //How the game should handle a purchase opportunity
    public enum PurchaseFlow
    {
        None,          //do nothing
        OfferToPlayer, //show yes/no in UI
        AutoBuy,       //buy immediately if allowed
        HookAction     //placeholder for logic like auction 
    }

    //Decision returned by purchase strategy
    public struct PurchaseDecision
    {
        public PurchaseFlow Flow;
        public int Price;
        public bool CanAfford;
    }

    //Centralized rules for deciding if and how a purchase should occur
    public interface IPurchaseStrategy
    {
        //Returns how the rules say to handle this purchase scenario
        //Uses same tile adapter interface as rent for now
        PurchaseDecision GetPurchaseDecision(
            ITileRentInfo tile,
            Player buyer,
            IOwnershipService own,
            IRuleSet rules);
    }
}
