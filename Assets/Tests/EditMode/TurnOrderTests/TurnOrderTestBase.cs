using NUnit.Framework;
using UnityEngine;
using Tests.EditMode;
using Assets.Scripts.Managers.TurnOrder;

namespace Tests.EditMode.TurnOrderTests
{
    public class TurnOrderTestBase : ManagerTestBase
    {
        protected TurnCycleManager cycle;

        [SetUp]
        public virtual void SetUp()
        {
            cycle = new TurnCycleManager(4);
        }

        [TearDown]
        public virtual void TearDown()
        {
            
        }
    }
}
