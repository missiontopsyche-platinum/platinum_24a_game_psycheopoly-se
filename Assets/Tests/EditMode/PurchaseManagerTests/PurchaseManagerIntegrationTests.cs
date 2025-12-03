using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Purchase;
using Assets.Scripts.Managers.Rules;

namespace Tests.EditMode.PurchaseTests
{
    //Checks that PurchaseManager spends money and updates ownership
    //using StandardPurchaseStrategy and OwnershipServiceAdapter.
    public class PurchaseManagerIntegrationTests : PurchaseTestBase
    {
        [Test]
        public void Unowned_Street_PlayerBuys_Property()
        {
            //Given an unowned street tile
            var prop = Street("Prop A", ColorGroup.Red, baseRent: 10, mortgaged: false);

            int beforeBuyer = buyer.GetMoney();

            //Use StandardPurchaseStrategy directly to get expected price
            var strat = new StandardPurchaseStrategy();
            var rules = new StandardRuleSet();
            var decision = strat.GetPurchaseDecision(prop, buyer, ownership, rules);

            Assert.AreEqual(PurchaseFlow.OfferToPlayer, decision.Flow,
                "Precondition: standard rules should offer purchase here.");
            int expectedPrice = decision.Price;

            //When buyer lands and PurchaseManager handles the purchase
            purchaseManager.TryHandlePurchase(buyer, prop);

            //buyers money decreases by expectedPrice
            Assert.AreEqual(beforeBuyer - expectedPrice, buyer.GetMoney());

            //buyer becomes the owner
            var actualOwner = ownership.GetOwner(prop);
            Assert.AreEqual(buyer, actualOwner);
        }

        [Test]
        public void Tile_AlreadyOwnedByOther_NoPurchase()
        {
            var prop = Street("Owned", ColorGroup.LightBlue, baseRent: 20);
            ownership.SetOwner(prop, other);

            int beforeBuyerMoney = buyer.GetMoney();
            var beforeOwner = ownership.GetOwner(prop);

            purchaseManager.TryHandlePurchase(buyer, prop);

            Assert.AreEqual(beforeBuyerMoney, buyer.GetMoney(),
                "Buyer should not spend money when tile already owned.");
            Assert.AreEqual(beforeOwner, ownership.GetOwner(prop),
                "Ownership should not change.");
        }

        [Test]
        public void Buyer_CannotAfford_NoPurchase()
        {
            var prop = Street("Expensive", ColorGroup.Green, baseRent: 500);
            buyer.SetMoney(0); //force cannot afford

            int beforeBuyer = buyer.GetMoney();

            purchaseManager.TryHandlePurchase(buyer, prop);

            Assert.AreEqual(beforeBuyer, buyer.GetMoney(),
                "Buyer should not spend money when they cannot afford.");

            Assert.IsNull(ownership.GetOwner(prop),
                "Owner should remain null when purchase fails.");
        }
    }
}
