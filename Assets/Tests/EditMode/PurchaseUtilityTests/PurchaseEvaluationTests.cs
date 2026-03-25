using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Purchase;
using Tests.EditMode.PlayerControllerTests.Builders;

namespace Tests.EditMode.PurchaseUtilityTests
{
    public class PurchaseEvaluationTests
    {
        [Test]
        public void Unowned_AffordableStreet_OffersToPlayer_UsesPurchasePrice()
        {
            var player = new MockPlayerBuilder().WithMoney(1500).Build();
            var prop = new MockOwnableBuilder().WithBuyPrice(120).BuildAsProperty();

            var evaluation = PurchaseUtility.EvaluatePurchase(prop, player);

            Assert.AreEqual(PurchaseFlow.OfferToPlayer, evaluation.Flow);
            Assert.IsTrue(evaluation.CanAfford);
            Assert.AreEqual(120, evaluation.Price,
                "Expected strategy to use PurchasePrice from tile (adapter data) rather than a rent-based fallback.");

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(prop);
        }

        [Test]
        public void TileOwnedByOtherPlayer_NoOffer()
        {
            var prop = new MockOwnableBuilder().BuildAsProperty();
            var p1 = new MockPlayerBuilder().WithMoney(1500).Build();
            var p2 = new MockPlayerBuilder().WithOwnedProperty(prop).Build();
            prop.SetOwner(p2);

            var decision = PurchaseUtility.EvaluatePurchase(prop, p1);

            Assert.AreEqual(PurchaseFlow.None, decision.Flow);
            Assert.IsFalse(decision.CanAfford);

            Object.DestroyImmediate(p1);
            Object.DestroyImmediate(p2);
            Object.DestroyImmediate(prop);
        }

        [Test]
        public void BuyerCannotAfford_NoOffer()
        {
            var player = new MockPlayerBuilder().WithMoney(100).Build();
            var prop = new MockOwnableBuilder().WithBuyPrice(200).BuildAsProperty();

            var decision = PurchaseUtility.EvaluatePurchase(prop, player);

            Assert.AreEqual(PurchaseFlow.None, decision.Flow);
            Assert.IsFalse(decision.CanAfford);

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(prop);
        }
    }
}
