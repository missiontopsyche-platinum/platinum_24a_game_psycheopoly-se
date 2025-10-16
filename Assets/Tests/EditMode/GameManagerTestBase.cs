using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerTestBase
{
    // Base test object
    protected GameObject gameObject;
    protected GameManager gameManager;
    
    // Event channels
    protected GameStateChangedEventChannel gameStateEventChannel;
    protected PlayerMovedEventChannel playerMovedChannel;
    protected TurnStartedEventChannel turnStartedChannel;
    protected IntEventChannel initializePlayerCountChannel;
    
    [SetUp]
    public virtual void SetUp()
    {
        // create and populate test GameObject
        gameObject = new GameObject("GameManagerTests");
        gameManager = gameObject.AddComponent<GameManager>();
        
        // create and add event channels
        gameStateEventChannel = ScriptableObject.CreateInstance<GameStateChangedEventChannel>();
        gameManager.gameStateChangedChannel = gameStateEventChannel;
        playerMovedChannel = ScriptableObject.CreateInstance<PlayerMovedEventChannel>();
        gameManager.playerMovedChannel = playerMovedChannel;
        turnStartedChannel = ScriptableObject.CreateInstance<TurnStartedEventChannel>();
        gameManager.turnStartedChannel = turnStartedChannel;
        initializePlayerCountChannel = ScriptableObject.CreateInstance<IntEventChannel>();
        gameManager.initializePlayerCountChannel = initializePlayerCountChannel;
    }

    [TearDown]
    public virtual void TearDown()
    {
        // destroy object
        if (gameObject != null)
            Object.DestroyImmediate(gameObject);
        
        // destroy event channels
        DestroyScriptableObject(gameStateEventChannel);
        DestroyScriptableObject(playerMovedChannel);
        DestroyScriptableObject(turnStartedChannel);
        DestroyScriptableObject(initializePlayerCountChannel);
    }

    private void DestroyScriptableObject(ScriptableObject channel)
    {
        if (channel != null)
            Object.DestroyImmediate(channel);
    }
}
