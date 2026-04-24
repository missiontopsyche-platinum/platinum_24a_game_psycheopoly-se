using NUnit.Framework;
using System.Collections;
using Tests.EditMode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using BoardManager = PsycheOpoly.Board.BoardManager;

namespace Tests.PlayMode {
    public class PlayerPanelControllerPlayModeTests : PlayTestBase
    {
        private GameObject root;
        private PlayerPanelController controller;
        [SetUp]
        public void SetUp()
        {
           
            Logging.Logger.Trace("DicePanelControllerPlayModeTests.SetUp",
                "Setting up DicePanelController PlayMode test",
                Logging.LogCategory.UI,
                this);

            //Create an on sceneLoaded event handler to build objects
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("PlayTestScene", LoadSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            //DestroyTestObjects(root, controller);
        }

        // [UnityTest]
        // public IEnumerator DisplayCurrentPlayer_UpdateTextFields()
        // {
        //     yield return new WaitWhile(() => !sceneLoaded); // wait for scene to be fully loaded
        //
        //     ClearAllPlayers();
        //     
        //     Assert.IsNotNull(controller.playerNameText, "Player name text should not be null before enabling.");
        //     Assert.IsNotNull(controller.playerMoneyText, "Player money text should not be null before enabling.");
        //
        //     var player1 = ScriptableObject.CreateInstance<Player>();
        //     player1.SetId(0);
        //     player1.SetPName("Alice");
        //     player1.SetMoney(1500);
        //
        //     //root.SetActive(true);
        //     controller.addPlayerEventChannel.RaiseEvent(player1);
        //
        //     controller.turnStartedChannel.RaiseEvent(new TurnStartedEvent(0, 1));
        //     yield return null;
        //
        //     Assert.AreEqual("Player: Alice", controller.playerNameText.text);
        //     Assert.AreEqual("Money: 1500", controller.playerMoneyText.text);
        // }

        protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            root = GameObject.Find("HUD").transform.Find("HUDRoot").Find("PlayerPanel").Find("PlayerPanelController").gameObject;
            //root.SetActive(false);
            controller = root.GetComponent<PlayerPanelController>();

            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>() as BoardManager;
            if (boardManager.ClearPlayers())
            {
                Logging.Logger.Trace("DicePanelControllerPlayModeTests.OnSceneLoaded",
                "Succesfully cleared boardManagers",
                Logging.LogCategory.UI,
                this);
            }
            if (boardManager.boardRenderer.ClearPlayers())
            {
                Logging.Logger.Trace("DicePanelControllerPlayModeTests.OnSceneLoaded",
                "Succesfully cleared boardRenderer",
                Logging.LogCategory.UI,
                this);
            }

            sceneLoaded = true;
        }
    }
}


