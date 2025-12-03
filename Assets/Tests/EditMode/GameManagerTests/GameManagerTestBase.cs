using Assets.Scripts.Managers.TurnOrder;
using Logging;
using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTestBase : ManagerTestBase
    {
        // Base test object
        protected GameObject gameObject;
        protected global::GameManager gameManager;

        [SetUp]
        public virtual void SetUp()
        {
            // create and populate test GameObject
            gameObject = new GameObject("GameManagerTests");
            gameManager = gameObject.AddComponent<global::GameManager>();

            // REQUIRED FOR US395 TURN SYSTEM ----
            var turnCycleGO = new GameObject("TurnCycleManager");
            var turnCycle = turnCycleGO.AddComponent<TurnCycleManager>();

            var turnStateGO = new GameObject("PlayerTurnState");
            var turnState = turnStateGO.AddComponent<PlayerTurnState>();

            // let GameManager discover these using FindFirstObjectByType in Awake()
            turnCycleGO.transform.SetParent(gameObject.transform.parent);
            turnStateGO.transform.SetParent(gameObject.transform.parent);

            //this is needed bc Awake() doesn't run in EditMode tests to GameManager can't access
            //it, so we assign a "reflection" of it to be accessible in EditMode
            var gmType = typeof(global::GameManager);

            var tcmField = gmType.GetField("turnCycleManager", BindingFlags.NonPublic | BindingFlags.Instance);
            tcmField?.SetValue(gameManager, turnCycle);

            var ptsField = gmType.GetField("playerTurnState", BindingFlags.NonPublic | BindingFlags.Instance);
            ptsField?.SetValue(gameManager, turnState);

            // create and add event channels
            gameManager.gameStateChangedChannel = CreateChannel<GameStateChangedEventChannel>();
            gameManager.turnStartedChannel = CreateChannel<TurnStartedEventChannel>();
            gameManager.playerMovedChannel = CreateChannel<PlayerMovedEventChannel>();
            gameManager.initializePlayerCountChannel = CreateChannel<IntEventChannel>();
            gameManager.diceRolledChannel = CreateChannel<DiceRolledEventChannel>();

            // subscribe to event channels
            gameManager.diceRolledChannel.Subscribe(gameManager.DiceRolled);

            gameManager.movePlayerChannel = CreateChannel<MovePlayerEventChannel>();
            gameManager.pieceMoveCompletedChannel = CreateChannel<BooleanEventChannel>();
            gameManager.rollDiceRequestedChannel = CreateChannel<BooleanEventChannel>();
            gameManager.cardDrawnChannel = CreateChannel<CardDrawnEventChannel>();
            gameManager.turnEndedChannel = CreateChannel<BooleanEventChannel>();


            InitializeTestLogger();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(gameObject);
        }
    }
}
