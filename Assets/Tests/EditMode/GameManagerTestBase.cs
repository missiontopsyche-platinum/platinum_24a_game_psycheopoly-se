using NUnit.Framework;
using UnityEngine;

public class GameManagerTestBase : ManagerTestBase
{
    // Base test object
    protected GameObject gameObject;
    protected GameManager gameManager;
    
    [SetUp]
    public virtual void SetUp()
    {
        // create and populate test GameObject
        gameObject = new GameObject("GameManagerTests");
        gameManager = gameObject.AddComponent<GameManager>();
        
        // create and add event channels
        gameManager.gameStateChangedChannel = CreateChannel<GameStateChangedEventChannel>();
        gameManager.turnStartedChannel = CreateChannel<TurnStartedEventChannel>();
        gameManager.playerMovedChannel = CreateChannel<PlayerMovedEventChannel>();
        gameManager.initializePlayerCountChannel = CreateChannel<IntEventChannel>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        DestroyTestObjects(gameObject);
    }
}
