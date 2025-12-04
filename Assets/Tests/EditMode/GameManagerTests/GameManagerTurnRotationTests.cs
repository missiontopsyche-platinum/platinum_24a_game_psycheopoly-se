using Assets.Scripts.Managers.TurnOrder;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTurnRotationTests : GameManagerTestBase
    {
    
        private int lastReceivedPlayer = -1;

        /// <summary>
        /// a helper used to simulate TurnFlowCoordinator:
        /// advance turn > fire TurnStartedEvent manually.
        /// </summary>
        private void AdvanceAndFireTurn(int turnNumber)
        {
            var turnCycle = UnityEngine.Object.FindFirstObjectByType<TurnCycleManager>();
            Assert.IsNotNull(turnCycle, "TurnCycleManager not found in scene during test.");

            int next = turnCycle.Advance();
            gameManager.turnStartedChannel.RaiseEvent(new TurnStartedEvent(next, turnNumber));
        }

        [Test]
        public void NextTurn_LoopBackWithTwoPlayers()
        {
            int callbackCount = 0;

            void Listener(TurnStartedEvent tse)
            {
                callbackCount++;
                lastReceivedPlayer = tse.playerId;
            }

            gameManager.turnStartedChannel.Subscribe(Listener);

            gameManager.Initialize();
            gameManager.SetUpGame(2);

            gameManager.CompleteGameInit(); // fires TurnStartedEvent(0,0)

            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);


            AdvanceAndFireTurn(1);
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer);

            AdvanceAndFireTurn(2);
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        }

        [Test]
        public void NextTurn_LoopBackWithThreePlayers()
        {
            int callbackCount = 0;

            void Listener(TurnStartedEvent tse)
            {
                callbackCount++;
                lastReceivedPlayer = tse.playerId;
            }

            gameManager.turnStartedChannel.Subscribe(Listener);


            gameManager.Initialize();
            gameManager.SetUpGame(3);
            gameManager.CompleteGameInit(); // fires TurnStartedEvent(0,0)

            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);


            // player 1
            AdvanceAndFireTurn(1);
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer);

            // player 2
            AdvanceAndFireTurn(2);
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(2, lastReceivedPlayer);

            // back to player 0
            AdvanceAndFireTurn(3);
            Assert.AreEqual(4, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        }

        [Test]
        public void NextTurn_LoopBackWithFourPlayers()
        {
            int callbackCount = 0;

            void Listener(TurnStartedEvent tse)
            {
                callbackCount++;
                lastReceivedPlayer = tse.playerId;
            }

            gameManager.turnStartedChannel.Subscribe(Listener);

            gameManager.Initialize();
            gameManager.SetUpGame(4);
            gameManager.CompleteGameInit(); // fires TurnStartedEvent(0,0)

            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);


            // player 1
            AdvanceAndFireTurn(1);
            Assert.AreEqual(2, callbackCount);
            Assert.AreEqual(1, lastReceivedPlayer);

            // player 2
            AdvanceAndFireTurn(2);
            Assert.AreEqual(3, callbackCount);
            Assert.AreEqual(2, lastReceivedPlayer);

            // player 3
            AdvanceAndFireTurn(3);
            Assert.AreEqual(4, callbackCount);
            Assert.AreEqual(3, lastReceivedPlayer);

            // loop back > player 0
            AdvanceAndFireTurn(4);
            Assert.AreEqual(5, callbackCount);
            Assert.AreEqual(0, lastReceivedPlayer);
        }

        [Test]
        public void SetUpGame_LogsErrorWhenInvalidPlayerCount()
        {
            gameManager.Initialize();
            gameManager.SetUpGame(1);
            var pattern = CreateRegexLogPattern("Error", "Gameplay", "GameManager.SetUpGame", "Invalid player count, must be between 2 and 4.");
            LogAssert.Expect(UnityEngine.LogType.Error, pattern);

            gameManager.Initialize();
            gameManager.SetUpGame(5);
            pattern = CreateRegexLogPattern("Error", "Gameplay", "GameManager.SetUpGame", "Invalid player count, must be between 2 and 4.");
            LogAssert.Expect(UnityEngine.LogType.Error, pattern);
        }
    }
}