using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode
{   
    public class PlayerTests
    {
        private Player player;

        [SetUp]
        public void SetUp()
        {
            player = new Player();
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
            Color32 testColor = new Color32(255, 0, 0, 255);
            player.SetColor(testColor);
            player.SetPosition(1);

            Assert.AreEqual(0, player.GetId());
            Assert.AreEqual("Test", player.GetPName());
            Assert.AreEqual(100, player.GetMoney());
            Assert.AreEqual(testColor, player.GetColor());
            Assert.AreEqual(1, player.GetPosition());
        }
    }
}
