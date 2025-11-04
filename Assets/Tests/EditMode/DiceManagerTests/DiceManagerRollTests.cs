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
        /// Currently tests 10 total calls. Can adjust as necessary
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

        /// <summary>
        /// Tests that the dice do not return the exact same results 3 times in a row
        /// Test is stringent. To count as the same result, both the individual dice and the total must be the same
        /// </summary>
        [Test]
        public void DiceRollIsRandom()
        {
            int counter = 0;
            int total = 0;
            int dieOne = 0;
            int dieTwo = 0;

            DiceRolledEvent dre = diceManager.RollDice();

            total = dre.totalRoll;
            dieOne = dre.dieOne;
            dieTwo = dre.dieTwo;
            counter = 1;

            for (int i = 0; i < 5; i++)
            {
                dre = diceManager.RollDice();
                if (dre.totalRoll == total && dre.dieOne == dieOne && dre.dieTwo == dieTwo)
                {
                    counter++;
                } else
                {
                    total = dre.totalRoll;
                    dieOne = dre.dieOne;
                    dieTwo = dre.dieTwo;
                    counter = 1;
                }
            }
            Assert.Less(counter, 3);
        }
    }
}
