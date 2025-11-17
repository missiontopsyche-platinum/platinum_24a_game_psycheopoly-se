using NUnit.Framework;
using UnityEngine;
using Tests.EditMode;
using Assets.Scripts.Managers.TurnOrder;

namespace Tests.EditMode.TurnOrderTests
{
    public class TurnOrderTestBase : ManagerTestBase
    {
        protected GameObject go;
        protected TurnCycleManager cycle;
        protected PlayerTurnState  state;

        [SetUp]
        public virtual void SetUp()
        {
            go    = new GameObject("TurnCycle");
            cycle = go.AddComponent<TurnCycleManager>();
            state = go.AddComponent<PlayerTurnState>();
            //Reset manager with 4 players and starts at 0
            cycle.ResetCycle(4, 0);
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(go);
        }
    }
}
