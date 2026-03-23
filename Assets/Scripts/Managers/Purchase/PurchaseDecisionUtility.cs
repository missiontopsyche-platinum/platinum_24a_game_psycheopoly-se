using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;

namespace Assets.Scripts.Managers.Purchase
{
    //Standard Monopoly purchase logic
    //Only unowned and purchasable tiles are offered for purchase
    //Player must be able to afford the price
    public static class PurchaseDecisionUtility
    {
        public static PurchaseDecision GetPurchaseDecision(
            OwnableSpaceData ownableSpace,
            Player buyer)
        {
            //Null and invalid checks
            if (ownableSpace == null || buyer == null)
                return new PurchaseDecision { Flow = PurchaseFlow.None };

            // We don't need to check 'valid types' if we're passing in Ownable Space Data because it
            // is inherently ownable- there is no way we can pass in a space that is not purchasable.

            if (ownableSpace.isMortgaged)
                return new PurchaseDecision { Flow = PurchaseFlow.None };

            //Checks if already owned by someone else
            //If so then No purchase
            var currentOwner = ownableSpace.GetOwner();
            if (currentOwner != null && currentOwner != buyer)
                return new PurchaseDecision { Flow = PurchaseFlow.None };

            int price = ownableSpace.buyPrice;

            int have = buyer.GetMoney();
            bool canAfford = have >= price;

            if (!canAfford)
            {
                return new PurchaseDecision
                {
                    Flow = PurchaseFlow.None,
                    Price = price,
                    CanAfford = false
                };
            }

            //Standard Monopoly rules so offer to the landing player
            return new PurchaseDecision
            {
                Flow = PurchaseFlow.OfferToPlayer,
                Price = price,
                CanAfford = true
            };
        }
    }
}
