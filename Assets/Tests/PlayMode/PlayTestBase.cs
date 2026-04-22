using Logging;
using NUnit.Framework;
using PsycheOpoly.Board;
using System.Collections;
using Tests.EditMode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Logging.Logger;

namespace Tests.PlayMode
{
    public class PlayTestBase : ManagerTestBase
    {
        protected bool sceneLoaded = false;

        [SetUp]
        public void Setup()
        {
            //Init logger
            InitializeTestLogger();
            Logging.Logger.Trace("PlayTestBase.SetUp",
                "Setting up base play test",
                Logging.LogCategory.UI,
                this);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadScene("PlayTestScene");
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        }

        // Call this on any test that involves adding players.
        protected void ClearAllPlayers()
        {
            BoardManager boardManager = GameObject.Find("Board").GetComponent<BoardManager>() as BoardManager;
            boardManager.ClearPlayers();
            boardManager.boardRenderer.ClearPlayers();
        }
    }
}

