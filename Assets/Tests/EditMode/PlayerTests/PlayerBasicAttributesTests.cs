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
    }
}
