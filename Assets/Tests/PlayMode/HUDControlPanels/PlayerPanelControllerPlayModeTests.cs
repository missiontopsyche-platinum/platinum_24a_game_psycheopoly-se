using NUnit.Framework;
using System.Collections;
using Tests.EditMode;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlayerPanelControllerPlayModeTests : ManagerTestBase
{
    private GameObject root;
    private PlayerPanelController controller;   
    [SetUp]
    public void SetUp()
    {
        InitializeTestLogger();
        Logging.Logger.Trace("DicePanelControllerPlayModeTests.SetUp",
            "Setting up DicePanelController PlayMode test", 
            Logging.LogCategory.UI,
            this);

        root = new GameObject("PlayerPanelControllerPlayModeTests");
        root.SetActive(false);
        controller = root.AddComponent<PlayerPanelController>();

        controller.playerNameText = CreateAndAttachComponent<Text>("PlayerNameText", root);
        controller.playerMoneyText = CreateAndAttachComponent<Text>("PlayerMoneyText", root);

        controller.turnStartedChannel = CreateChannel<TurnStartedEventChannel>();
        controller.playerEventChannel = CreateChannel<PlayerEventChannel>();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestObjects(root, controller);
    }

    [UnityTest]
    public IEnumerator DisplayCurrentPlayer_UpdateTextFields()
    {
        Assert.IsNotNull(controller.playerNameText, "Player name text should not be null before enabling.");
        Assert.IsNotNull(controller.playerMoneyText, "Player money text should not be null before enabling.");

        var player1 = ScriptableObject.CreateInstance<Player>();
        player1.SetId(0);
        player1.SetPName("Alice");
        player1.SetMoney(1500);

        root.SetActive(true);
        controller.playerEventChannel.RaiseEvent(player1);

        controller.turnStartedChannel.RaiseEvent(new TurnStartedEvent(0, 1));
        yield return null;

        Assert.AreEqual("Player: Alice", controller.playerNameText.text);
        Assert.AreEqual("Money: 1500", controller.playerMoneyText.text);
    }
}
