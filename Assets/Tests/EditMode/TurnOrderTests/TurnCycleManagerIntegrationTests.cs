using NUnit.Framework;
using Assets.Scripts.Managers.TurnOrder;

namespace Tests.EditMode.TurnOrderTests
{
    public class TurnCycleManagerIntegrationTests : TurnOrderTestBase
    {
        [Test]
        public void Advance_Normal_Sequence()
        {
            Assert.AreEqual(1, cycle.Advance()); //from 0 to 1
            Assert.AreEqual(2, cycle.Advance()); //from 1 to 2
            Assert.AreEqual(3, cycle.Advance()); //from 2 to 3
            Assert.AreEqual(0, cycle.Advance()); //from 3 to 0, wrap
        }

        [Test]
        public void Advance_Honors_ExtraTurn()
        {
            cycle.GrantExtraTurn(0);
            Assert.AreEqual(0, cycle.Advance()); //extra turn for 0
            Assert.AreEqual(1, cycle.Advance()); //continue 
        }

        [Test]
        public void Advance_Consumes_Skip()
        {
            cycle.AddSkip(1, 1);
            Assert.AreEqual(2, cycle.Advance()); //1 is skipped
            Assert.AreEqual(3, cycle.Advance()); //from 2 to 3
            Assert.AreEqual(0, cycle.Advance()); //from 3 to 0
            Assert.AreEqual(1, cycle.Advance()); //from 0 to 1, skip is used
        }

        [Test]
        public void Advance_Skips_Eliminated()
        {
            cycle.Eliminate(1);
            Assert.AreEqual(2, cycle.Advance()); //from 0 to 2
            Assert.AreEqual(3, cycle.Advance()); //from 2 to 3
            Assert.AreEqual(0, cycle.Advance()); //from 3 to 0
        }
    }
}
