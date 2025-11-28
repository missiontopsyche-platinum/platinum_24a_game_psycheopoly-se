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
        [SetUp]
        public void Setup()
        {
            //Init logger
            InitializeTestLogger();

            SceneManager.LoadScene("PlayTestScene");
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadScene("PlayTestScene");
        }


        
    }
}

