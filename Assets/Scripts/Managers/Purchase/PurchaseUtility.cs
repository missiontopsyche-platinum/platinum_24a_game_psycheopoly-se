using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;
using Logging;

namespace Assets.Scripts.Managers.Purchase
{
    // Static utility to validate and execute purchases of property.
    public static class PurchaseUtility
    {
        /// <summary>
        /// This method evaluates the validity of the purchase, letting the system know if it is a
        /// viable purchase or not, allowing for a decision to be made on the player's end.
        /// </summary>
        /// <param name="ownableSpace">The space up for purchase</param>
        /// <param name="buyer">The player trying to buy</param>
        /// <returns>PurchaseEvaluation struct with the result</returns>
        public static PurchaseEvaluation EvaluatePurchase(
            OwnableSpaceData ownableSpace,
            Player buyer)
        {
            //Null and invalid checks
            if (ownableSpace == null || buyer == null)
                return new PurchaseEvaluation { Flow = PurchaseFlow.None };

            // We don't need to check 'valid types' if we're passing in Ownable Space Data because it
            // is inherently ownable- there is no way we can pass in a space that is not purchasable.

            if (ownableSpace.isMortgaged)
                return new PurchaseEvaluation { Flow = PurchaseFlow.None };

            //Checks if already owned by someone else
            //If so then No purchase
            var currentOwner = ownableSpace.GetOwner();
            if (currentOwner != null && currentOwner != buyer)
                return new PurchaseEvaluation { Flow = PurchaseFlow.None };

            int price = ownableSpace.buyPrice;

            int have = buyer.GetMoney();
            bool canAfford = have >= price;

            if (!canAfford)
            {
                return new PurchaseEvaluation
                {
                    Flow = PurchaseFlow.None,
                    Price = price,
                    CanAfford = false
                };
            }

            //Standard Monopoly rules so offer to the landing player
            return new PurchaseEvaluation
            {
                Flow = PurchaseFlow.OfferToPlayer,
                Price = price,
                CanAfford = true
            };
        }
        
        /// <summary>
        /// Attempts to execute a purchase. Ideally, this is a valid purchase and the player can afford it.
        /// </summary>
        /// <param name="ownableSpace">The space up for purchase</param>
        /// <param name="buyer">The player trying to buy</param>
        /// <returns>true if the purchase is successful, false if not.</returns>
        public static bool ExecutePurchase(OwnableSpaceData ownableSpace, Player buyer)
        {
            if(!buyer.CanAfford(ownableSpace.buyPrice))
            {
                Logging.Logger.Error("PurchaseManager.ExecutePurchase",
                    "Player does not have enough money.",
                    LogCategory.Gameplay);
                return false;
            }

            buyer.TrySpend(ownableSpace.buyPrice);
            ownableSpace.SetOwner(buyer);
            buyer.AddOwnedProperty(ownableSpace);
            
            return true;
        }
    }
}
