using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTestBase : ManagerTestBase
    {
        // Base test object
        protected GameObject gameObject;
        protected global::GameManager gameManager;
    
        [SetUp]
        public virtual void SetUp()
        {
            // create and populate test GameObject
            gameObject = new GameObject("GameManagerTests");
            gameManager = gameObject.AddComponent<global::GameManager>();
        
            // create and add event channels
            gameManager.gameStateChangedChannel = CreateChannel<GameStateChangedEventChannel>();
            gameManager.turnStartedChannel = CreateChannel<TurnStartedEventChannel>();
            gameManager.playerMovedChannel = CreateChannel<PlayerMovedEventChannel>();
            gameManager.initializePlayerCountChannel = CreateChannel<IntEventChannel>();
            gameManager.diceRolledChannel = CreateChannel<DiceRolledEventChannel>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(gameObject);
        }
    }
}
