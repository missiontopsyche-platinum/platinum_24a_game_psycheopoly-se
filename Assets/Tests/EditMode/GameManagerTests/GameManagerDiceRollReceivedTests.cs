using UnityEngine;
using NUnit.Framework;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerDiceRollReceivedTests : GameManagerTestBase
    {
        [Test]
        public void GamerManagerReceivesDiceRollResults()
        {
            // send an event to the DiceRolled event. No randomized numbers used
            gameManager.diceRolledChannel?.RaiseEvent(new DiceRolledEvent(1, 1, 2));
            
            // Verify they are what was passed.
            Assert.AreEqual(1, gameManager.dieOne);
            Assert.AreEqual(1, gameManager.dieTwo);
            Assert.AreEqual(2, gameManager.totalRolled);
        }
    }
}

