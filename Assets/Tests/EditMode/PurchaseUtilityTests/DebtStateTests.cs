using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PlayerTests
{
    public class DebtStateTests
    {
        private Player player;

        [SetUp]
        public void SetUp()
        {
            player = ScriptableObject.CreateInstance<Player>();
            player.SetPName("Debt Test Player");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void AddMoneyIncreasesPlayerMoney()
        {
            player.SetMoney(100);

            player.AddMoney(40);

            Assert.AreEqual(140, player.GetMoney());
        }

        [Test]
        public void AddMoneyFromZero()
        {
            player.SetMoney(0);

            player.AddMoney(60);

            Assert.AreEqual(60, player.GetMoney());
        }

        [Test]
        public void AddMoneyAfterSuccessfulSpend()
        {
            player.SetMoney(200);
            player.TrySpend(50);

            player.AddMoney(25);

            Assert.AreEqual(175, player.GetMoney());
        }
    }
}