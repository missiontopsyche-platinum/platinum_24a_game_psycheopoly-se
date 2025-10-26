using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Tests.EditMode;

public class TurnPanelControllerPlayModeTests : ManagerTestBase
{
    private GameObject root;
    private TurnPanelController controller;

    [SetUp]
    public void SetUp()
    {
        InitializeTestLogger();
        Logging.Logger.Trace("TurnPanelControllerPlayModeTests.SetUp",
            "Setting up TurnPanelController PlayMode test", 
            Logging.LogCategory.UI,
            this);

        root = new GameObject("TurnPanelControllerPlayModeTests");
        root.SetActive(false);
        controller = root.AddComponent<TurnPanelController>();

        var turnNumberTextGO = new GameObject("TurnNumberText");
        turnNumberTextGO.transform.SetParent(root.transform);
        controller.turnNumberText = turnNumberTextGO.AddComponent<Text>();

        controller.turnStartedChannel = CreateChannel<TurnStartedEventChannel>();
    }
    [TearDown]
    public void TearDown()
    {
        DestroyTestObjects(root, controller);
    }
    [UnityTest]
    public IEnumerator DisplayCurrentTurn_UpdatesTurnNumberText_OnEvent()
    {
        Assert.IsNotNull(controller.turnNumberText, "Turn number text should not be null before enabling.");
        root.SetActive(true);

        controller.turnStartedChannel.RaiseEvent(new TurnStartedEvent(2, 5));
        yield return null;

        Assert.AreEqual("Turn: 5", controller.turnNumberText.text);
    }
}
