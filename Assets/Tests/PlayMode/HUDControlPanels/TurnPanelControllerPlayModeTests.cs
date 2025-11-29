using NUnit.Framework;
using PsycheOpoly.Board;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;


namespace Tests.PlayMode
{
    public class TurnPanelControllerPlayModeTests : PlayTestBase
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

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("PlayTestScene", LoadSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [UnityTest]
        public IEnumerator DisplayCurrentTurn_UpdatesTurnNumberText_OnEvent()
        {
            yield return new WaitWhile(() => !sceneLoaded); // wait for scene to be fully loaded

            Assert.IsNotNull(controller.turnNumberText, "Turn number text should not be null before enabling.");
            root.SetActive(true);

            controller.turnStartedChannel.RaiseEvent(new TurnStartedEvent(2, 5));
            yield return null;

            Assert.AreEqual("Turn: 5", controller.turnNumberText.text);
        }

        protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            root = GameObject.Find("HUD").transform.Find("HUDRoot").Find("TurnPanel").Find("TurnPanelController").gameObject;
            //root.SetActive(false);
            controller = root.GetComponent<TurnPanelController>();
            sceneLoaded = true;
        }
    }
}

