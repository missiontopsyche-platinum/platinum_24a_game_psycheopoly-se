using NUnit.Framework;
using UnityEngine;

public class PlayerManagerTestBase : ManagerTestBase
{
    protected GameObject gameObject;
    protected PlayerManager playerManager;
    
    [SetUp]
    public virtual void SetUp()
    {
        gameObject = new GameObject("PlayerManagerTests");
        playerManager = gameObject.AddComponent<PlayerManager>();

        playerManager.playerAddedEventChannel = CreateChannel<PlayerEventChannel>();
        playerManager.playerRemovedEventChannel = CreateChannel<PlayerEventChannel>();
        playerManager.initializePlayerCountChannel = CreateChannel<IntEventChannel>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        DestroyTestObjects(gameObject);
    }
}
