using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.TurnOrder;

namespace Tests.EditMode.TurnOrderTests
{
    public class StandardTurnOrderStrategyTests
    {
        private ITurnOrderStrategy strat;
        private PlayerTurnState state;

        [SetUp]
        public void SetUp()
        {
            strat = new StandardTurnOrderStrategy();
            state = new GameObject("state").AddComponent<PlayerTurnState>();
            state.EnsureSize(4);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(state.gameObject);
        }

        [Test]
        public void Normal_Advance_Wraps()
        {
            Assert.AreEqual(1, strat.NextPlayerIndex(0, 4, state));
            Assert.AreEqual(2, strat.NextPlayerIndex(1, 4, state));
            Assert.AreEqual(3, strat.NextPlayerIndex(2, 4, state));
            Assert.AreEqual(0, strat.NextPlayerIndex(3, 4, state));
        }

        [Test]
        public void ExtraTurn_Repeats_And_Clears()
        {
            state.GrantExtraTurn(2);
            //First call stays at 2
            Assert.AreEqual(2, strat.NextPlayerIndex(2, 4, state));
            //flag should have been cleared
            // The next player should be 3
            Assert.AreEqual(3, strat.NextPlayerIndex(2, 4, state));
        }

        [Test]
        public void Skip_Consumes_And_Jumps()
        {
            state.AddSkip(1, 1);
            Assert.AreEqual(2, strat.NextPlayerIndex(0, 4, state)); //1 is skipped
            //next turn should be plauer 1 again
            Assert.AreEqual(1, strat.NextPlayerIndex(0, 4, state));
        }

        [Test]
        public void Eliminated_Is_Skipped_Permanently()
        {
            state.Eliminate(1);
            Assert.AreEqual(2, strat.NextPlayerIndex(0, 4, state));
            Assert.AreEqual(3, strat.NextPlayerIndex(2, 4, state));
        }
    }
}
