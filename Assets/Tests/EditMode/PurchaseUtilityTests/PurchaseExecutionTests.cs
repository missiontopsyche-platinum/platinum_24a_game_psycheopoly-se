using Assets.Scripts.Managers.Purchase;
using NUnit.Framework;
using Tests.EditMode.PlayerControllerTests.Builders;
using UnityEngine;

namespace Tests.EditMode.PurchaseUtilityTests
{
    public class PurchaseExecutionTests
    {
        [Test]
        public void PlayerCanAfford_ReturnsTrue_OwnershipChanged()
        {
            var player = new MockPlayerBuilder().WithMoney(1500).Build();
            var prop = new MockOwnableBuilder().WithBuyPrice(100).BuildAsProperty();

            bool result = PurchaseUtility.ExecutePurchase(prop, player);
            
            Assert.AreEqual(true, result);
            Assert.Contains(prop, player.GetOwnedProperties());
            Assert.AreEqual(player, prop.GetOwner());
            
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(prop);
        }

        [Test]
        public void PlayerCantAfford_ReturnsFalse_OwnershipDoesntChange()
        {
            var player = new MockPlayerBuilder().WithMoney(50).Build();
            var prop = new MockOwnableBuilder().WithBuyPrice(100).BuildAsProperty();
            
            bool result = PurchaseUtility.ExecutePurchase(prop, player);
            
            Assert.AreEqual(false, result);
            Assert.AreEqual(0, player.GetOwnedProperties().Count);
            Assert.AreEqual(null, prop.GetOwner());
            
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(prop);
        }
    }
}