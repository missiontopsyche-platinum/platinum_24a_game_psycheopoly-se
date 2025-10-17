using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.DiceManagerTests
{
    public class DiceManagerTestBase : ManagerTestBase
    {
        // Base Test Object
        protected GameObject gameObject;
        protected global::DiceManager diceManager;
        [SetUp]
        public virtual void SetUp()
        {
            gameObject = new GameObject("DiceManagerTests");
            diceManager = gameObject.AddComponent<global::DiceManager>();

            diceManager.diceRolledChannel = CreateChannel<DiceRolledEventChannel>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(gameObject);
        }

    }
}

