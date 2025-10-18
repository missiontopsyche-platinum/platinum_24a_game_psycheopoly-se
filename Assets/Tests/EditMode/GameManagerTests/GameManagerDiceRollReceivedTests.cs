using UnityEngine;
using NUnit.Framework;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerDiceRollReceivedTests : GameManagerTestBase
    {
        [Test]
        public void GamerManagerReceivesDiceRollResults()
        {
            gameManager.diceRolledChannel?.RaiseEvent(new DiceRolledEvent(1, 1, 2));
            

            Assert.AreEqual(1, gameManager.dieOne);
            Assert.AreEqual(1, gameManager.dieTwo);
            Assert.AreEqual(2, gameManager.totalRolled);
        }
    }
}

