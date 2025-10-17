using UnityEngine;
using NUnit.Framework;
namespace Tests.EditMode.DiceManagerTests 
{
    public class DiceManagerRollTests : DiceManagerTestBase
    {
        /// <summary>
        /// Tests that dice rolls are within range.
        /// Single dice values are between 1 and 6
        /// Total roll value between 2 and 12
        /// </summary>
        [Test]
        public void DiceRollIsInValidRange()
        {
            
            for (int i = 0; i < 10; i++)
            {
                DiceRolledEvent dre = diceManager.RollDice();
                Assert.GreaterOrEqual(dre.dieOne, 1);
                Assert.GreaterOrEqual(dre.dieTwo, 1);
                Assert.GreaterOrEqual(dre.totalRoll, 2);
                Assert.LessOrEqual(dre.dieOne, 6);
                Assert.LessOrEqual(dre.dieTwo, 6);
                Assert.LessOrEqual(dre.totalRoll, 12);
            }
            
        }
    }
}
