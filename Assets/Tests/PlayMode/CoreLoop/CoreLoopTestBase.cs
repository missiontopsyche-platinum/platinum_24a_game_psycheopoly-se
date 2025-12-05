using Logging;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BoardManager = PsycheOpoly.Board.BoardManager;
using Managers;

namespace Tests.PlayMode
{
    public class CoreLoopTestBase : PlayTestBase
    {
        //Managers
        GameManager gameManager;
        PlayerManager playerManager;
        BoardManager boardManager;
        DiceManager diceManager;
        InputManager inputManager;

        //Necessary Game objects
        GameObject hudRoot;

        //HUD Controllers
        DicePanelController dicePanelController;

        [SetUp]
        public void SetUp()
        {
            Logging.Logger.Trace("CoreLoopTestBase.SetUp",
                "Setting up core loop testing",
                Logging.LogCategory.Gameplay,
                this);

            //Create an on sceneLoaded event handler to build objects
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("CoreLoopTestScene", LoadSceneMode.Single);
        }

        protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Get all necessary managers.
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
            boardManager = GameObject.Find("Board").GetComponent<BoardManager>();
            diceManager = GameObject.Find("DiceManager").GetComponent<DiceManager>();
            inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

            //get the hud root object.
            hudRoot = GameObject.Find("HUD").transform.Find("HUDRoot").gameObject;

            //get the different necessary hud items from the hud root.
            dicePanelController = hudRoot.transform.Find("DicePanel").Find("DicePanelController").GetComponent<DicePanelController>();

            sceneLoaded = true;
        }
    }
}

