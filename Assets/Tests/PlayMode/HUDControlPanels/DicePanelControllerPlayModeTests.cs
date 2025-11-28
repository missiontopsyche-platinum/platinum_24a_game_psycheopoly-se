using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Tests.PlayMode
{
    public class DicePanelControllerPlayModeTests : PlayTestBase
    {
        private GameObject root;
        private DicePanelController controller;

        [SetUp]
        public void SetUp()
        {
            //Create an on sceneLoaded event handler to build objects
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logging.Logger.Trace("DicePanelControllerPlayModeTests.SetUp",
                "Setting up DicePanelController PlayMode test",
                Logging.LogCategory.UI,
                this);

        }

        [TearDown]
        public void TearDown()
        {
           
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

            var diceEvent = new DiceRolledEvent(2, 5, 7);
            controller.diceRolledChannel.RaiseEvent(diceEvent);

            yield return null; // waiting for one frame for listener to process

            Assert.AreEqual("Die One: 2", controller.dice1RolledText.text);
            Assert.AreEqual("Die Two: 5", controller.dice2RolledText.text);
            Assert.AreEqual("Total: 7", controller.diceTotalText.text);
        }

        [UnityTest]
        public IEnumerator RollButton_Click_Raises_RollDiceRequestedEvent()
        {

            yield return null; // wait a frame for the scene to fully load

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

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            root = GameObject.Find("HUD").transform.Find("HUDRoot").Find("DicePanel").Find("DicePanelController").gameObject;
            root.SetActive(false);
            controller = root.GetComponent<DicePanelController>();
            controller.rollDiceButton = CreateAndAttachComponent<Button>("Button", root);
        }
    }
}
