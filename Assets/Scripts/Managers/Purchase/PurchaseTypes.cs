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
    public struct PurchaseEvaluation
    {
        public PurchaseFlow Flow;
        public int Price;
        public bool CanAfford;
    }