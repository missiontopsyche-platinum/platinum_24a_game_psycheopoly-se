using NUnit.Framework;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTransitionsTest : GameManagerTestBase
    {
        [Test]
        public void EndGame_FromNone_IsBlocked()
        {
            var pattern = CreateRegexLogPattern("Warn", "Gameplay", "GameManager.SetState", "Illegal transition: None -> GameOver");
            LogAssert.Expect(LogType.Warning, pattern);
            gameManager.EndGame();
            Assert.AreEqual(GameState.None, gameManager.gameState);
        }

        [Test]
        public void Initialize_AllowsTransition_FromNone()
        {
            gameManager.Initialize();
            Assert.AreEqual(GameState.Initializing, gameManager.gameState);
        }
    }
}
