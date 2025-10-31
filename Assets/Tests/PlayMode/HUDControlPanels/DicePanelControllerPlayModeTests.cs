using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Tests.EditMode;

public class DicePanelControllerPlayModeTests : ManagerTestBase
{
    private GameObject root;
    private DicePanelController controller;

    [SetUp]
    public void SetUp()
    {
        InitializeTestLogger();
        Logging.Logger.Trace("DicePanelControllerPlayModeTests.SetUp",
            "Setting up DicePanelController PlayMode test", 
            Logging.LogCategory.UI,
            this);

        root = new GameObject("DicePanelControllerPlayModeTests");
        root.SetActive(false);
        controller = root.AddComponent<DicePanelController>();

        controller.rollDiceButton = CreateAndAttachComponent<Button>("RollDiceButton", root);
        controller.dice1RolledText = CreateAndAttachComponent<Text>("Dice1RolledText", root);
        controller.dice2RolledText = CreateAndAttachComponent<Text>("Dice2RolledText", root);
        controller.diceTotalText = CreateAndAttachComponent<Text>("DiceTotalText", root);

        controller.diceRolledRequestedChannel = CreateChannel<RollDiceRequestedEventChannel>();
        controller.diceRolledChannel = CreateChannel<DiceRolledEventChannel>();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestObjects(root, controller);
    }

    [UnityTest]
    public IEnumerator DisplayDiceRoll_UpdatesTexts_OnEvent()
    {
        // Sanity check
        Assert.IsNotNull(controller.rollDiceButton, "Roll button should not be null before enabling.");
        Assert.IsNotNull(controller.dice1RolledText);
        Assert.IsNotNull(controller.dice2RolledText);
        Assert.IsNotNull(controller.diceTotalText);

        // Enable GameObject
        root.SetActive(true);
        yield return null; // waiting for Unity finish enabling frame

        var diceEvent = new DiceRolledEvent (2, 5, 7 );
        controller.diceRolledChannel.RaiseEvent(diceEvent);

        yield return null; // waiting for one frame for listener to process

        Assert.AreEqual("Die One: 2", controller.dice1RolledText.text);
        Assert.AreEqual("Die Two: 5", controller.dice2RolledText.text);
        Assert.AreEqual("Total: 7", controller.diceTotalText.text);
    }

    [Test]
    public void RollButton_Click_Raises_RollDiceRequestedEvent()
    {
        bool received = false;
        controller.diceRolledRequestedChannel.Subscribe(v => received = v);

        // Sanity check
        Assert.IsNotNull(controller.rollDiceButton, "rollDiceButton should be assigned before enabling.");

        // Note: Activate object so OnEnable hooks listener
        root.SetActive(true);

        controller.rollDiceButton.onClick.Invoke();
        Assert.IsTrue(received, "RollDiceRequestedEventChannel to receive true when button click.");
    }

    [UnityTest]
    public IEnumerator RollButton_Click_WithNullChannel_DoesNotThrow()
    {
        controller.diceRolledRequestedChannel = null;
        Assert.DoesNotThrow(() => controller.rollDiceButton.onClick.Invoke());
        yield return null;
    }
}
