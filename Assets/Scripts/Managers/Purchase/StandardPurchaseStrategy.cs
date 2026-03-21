using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Rules;

namespace Assets.Scripts.Managers.Purchase
{
    //Standard Monopoly purchase logic
    //Only unowned and purchasable tiles are offered for purchase
    //Player must be able to afford the price
    public class StandardPurchaseStrategy : IPurchaseStrategy
    {
        public PurchaseDecision GetPurchaseDecision(
            ITileRentInfo tile,
            Player buyer,
            IOwnershipService own,
            StandardRuleSet rules)
        {
            //Null and invalid checks
            if (tile == null || buyer == null)
                return new PurchaseDecision { Flow = PurchaseFlow.None };

            //Only allow purchase on streets, railroads, and utilities
            if (tile.Type != TileType.Street &&
                tile.Type != TileType.Railroad &&
                tile.Type != TileType.Utility)
            {
                return new PurchaseDecision { Flow = PurchaseFlow.None };
            }

            if (tile.IsMortgaged)
                return new PurchaseDecision { Flow = PurchaseFlow.None };

            //Checks if already owned by someone else
            //If so then No purchase
            var currentOwner = own.GetOwner(tile);
            if (currentOwner != null && currentOwner != buyer)
                return new PurchaseDecision { Flow = PurchaseFlow.None };

            /// Prefer data-driven purchase price when available (OwnableSpaceData.buyPrice via IPurchasableTileInfo).
            // Fallback to 4x base rent for tiles that don't have purchase data wired yet.
            int price;

            if (tile is IPurchasableTileInfo purchasable)
            {
                price = Mathf.Max(0, purchasable.PurchasePrice);
            }
            else
            {
                price = Mathf.Max(0, tile.BaseRent * 4);
            }

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
