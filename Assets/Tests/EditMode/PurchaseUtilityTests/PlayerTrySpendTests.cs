using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PlayerTests
{
    public class PlayerTrySpendTests
    {
        private Player player;

        [SetUp]
        public void SetUp()
        {
            player = ScriptableObject.CreateInstance<Player>();
            player.SetPName("Test Player");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TrySpendWhenPlayerCanAffordReturnsSuccess()
        {
            player.SetMoney(500);

            Player.FinancialStatus result = player.TrySpend(100);

            Assert.AreEqual(Player.FinancialStatus.Success, result);
            Assert.AreEqual(400, player.GetMoney());
        }

        [Test]
        public void TrySpendWhenPlayerCannotAffordAndCannotCoverWithAssets()
        {
            player.SetMoney(50);

            Player.FinancialStatus result = player.TrySpend(500);

            Assert.AreEqual(Player.FinancialStatus.Bankrupt, result);
            Assert.AreEqual(50, player.GetMoney());
        }
    }
}