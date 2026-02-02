using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.PlayerTests
{
    public class PlayerBasicAttributesTests
    {
        private Player player;

        [SetUp]
        public void SetUp()
        {
            player = ScriptableObject.CreateInstance<Player>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void Player_CanInitialize()
        {
            player.SetId(0);
            player.SetPName("Test");
            player.SetMoney(100);
            Color testColor = new Color(1f, 0, 0, 1f);
            player.SetColor(testColor);
            player.SetPosition(1);

            Assert.AreEqual(0, player.GetId());
            Assert.AreEqual("Test", player.GetPName());
            Assert.AreEqual(100, player.GetMoney());
            Assert.AreEqual(testColor, player.GetColor());
            Assert.AreEqual(1, player.GetPosition());
        }

        [Test]
        public void Player_ThrowsMoneyError()
        {
            Assert.Throws<System.ArgumentException>(() => 
                player.SetMoney(-100));
        }

        [Test]
        public void Player_ThrowsPositionError()
        {
            Assert.Throws<System.ArgumentException>(() =>
                player.SetPosition(-100));
        }

        [Test]
        public void Player_CanAfford()
        {
            player.SetMoney(1000); // Assure player has money
            Assert.IsTrue(player.CanAfford(500));
        }

        [Test]
        public void Player_CanNotAfford()
        {
            player.SetMoney(1000); // Assure player has money
            Assert.IsFalse(player.CanAfford(1100));
        }

        [Test]
        public void Player_IsNotBankrupt()
        {
            player.SetMoney(1000);
            Assert.IsFalse(player.IsBankrupt());
        }

        [Test]
        public void Player_IsBankrupt()
        {
            player.SetMoney(0);
            Assert.IsTrue(player.IsBankrupt());
        }

        [Test]
        public void Player_CanSpend()
        {
            player.SetMoney(1000);
            Assert.IsTrue(player.TrySpend(500));
        }
        [Test]
        public void Player_CanNotSpend()
        {
            player.SetMoney(1000);
            Assert.IsFalse(player.TrySpend(1100));
        }
    }
}
