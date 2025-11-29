using Logging;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Logging.Logger;
using Tests.EditMode;

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

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
    }
}

