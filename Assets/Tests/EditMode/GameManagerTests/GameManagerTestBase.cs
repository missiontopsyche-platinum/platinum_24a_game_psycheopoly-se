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

            var tflowGO = new GameObject("TurnFlowCoordinator");
            var tflow = tflowGO.AddComponent<Assets.Scripts.Managers.TurnFlow.TurnFlowCoordinator>();
            tflowGO.SetActive(true);

            tflowGO.transform.SetParent(gameObject.transform.parent);

            //this is needed bc Awake() doesn't run in EditMode tests to GameManager can't access
            //it, so we assign a "reflection" of it to be accessible in EditMode
            var gmType = typeof(global::GameManager);

            // create and add event channels
            gameManager.gameStateChangedChannel = CreateChannel<GameStateChangedEventChannel>();
            gameManager.turnStartedChannel = CreateChannel<TurnStartedEventChannel>();
            gameManager.initializePlayerCountChannel = CreateChannel<IntEventChannel>();
            gameManager.diceRolledChannel = CreateChannel<DiceRolledEventChannel>();

            // subscribe to event channels
            gameManager.diceRolledChannel.Subscribe(gameManager.DiceRolled);

            gameManager.pieceMoveCompletedChannel = CreateChannel<BooleanEventChannel>();
            gameManager.cardDrawnChannel = CreateChannel<CardDrawnEventChannel>();
            gameManager.turnEndedChannel = CreateChannel<BooleanEventChannel>();
            gameManager.spaceResolutionCompletedChannel = CreateChannel<BooleanEventChannel>();

            // Wire channels into TurnFlowCoordinator (reflection)
            var tflowType = typeof(Assets.Scripts.Managers.TurnFlow.TurnFlowCoordinator);

            tflowType.GetField("turnStartedInChannel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tflow, gameManager.turnStartedChannel);

            tflowType.GetField("turnEndedChannel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tflow, gameManager.turnEndedChannel);

            tflowType.GetField("diceRolledChannel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tflow, gameManager.diceRolledChannel);

            tflowType.GetField("pieceMoveCompletedChannel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tflow, gameManager.pieceMoveCompletedChannel);

            tflowType.GetField("turnStartedOutChannel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tflow, gameManager.turnStartedChannel);

            tflowType.GetField("playerManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tflow, null); // optional for test

            tflowType.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)
    ?.Invoke(tflow, null);

            InitializeTestLogger();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(gameObject);
        }
    }
}
