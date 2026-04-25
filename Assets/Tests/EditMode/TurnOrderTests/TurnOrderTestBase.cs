using NUnit.Framework;
using UnityEngine;
using Tests.EditMode;
using Assets.Scripts.Managers.TurnOrder;
using System.Reflection;

namespace Tests.EditMode.TurnOrderTests
{
    public class TurnOrderTestBase : ManagerTestBase
    {
        protected TurnCycleManager cycle;
        private GameObject gameManagerGO;

        [SetUp]
        public virtual void SetUp()
        {
            gameManagerGO = new GameObject("GameManager");
            var gm = gameManagerGO.AddComponent<GameManager>();

            SetGameManagerInstance(gm);

            cycle = new TurnCycleManager(4);
        }

        [TearDown]
        public virtual void TearDown()
        {
            SetGameManagerInstance(null);

            if (gameManagerGO != null)
                Object.DestroyImmediate(gameManagerGO);
        }

        private static void SetGameManagerInstance(GameManager value)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            var field = typeof(GameManager).GetField("instance", flags);
            if (field != null)
            {
                field.SetValue(null, value);
                return;
            }

            var property = typeof(GameManager).GetProperty("instance", flags);
            if (property != null)
            {
                property.SetValue(null, value);
                return;
            }

            Assert.Fail("Could not find GameManager.instance as a field or property.");
        }
    }
}