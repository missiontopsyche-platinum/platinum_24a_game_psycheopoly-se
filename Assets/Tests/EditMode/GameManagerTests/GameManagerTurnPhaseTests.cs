using Assets.Scripts.Managers.Movement;
using Assets.Scripts.Managers.TurnOrder;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.EditMode.GameManagerTests
{
    public class GameManagerTurnPhaseTests : GameManagerTestBase
    {

        private TurnStartedEvent turnStartedEvent;

        private void SetPrivate<T>(string fieldName, T value)
        {
            var field = typeof(GameManager)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(gameManager, value);
        }

        [Test]
        public void CompleteGameInit_FromWaitingForTurn_SetsPlayerTurnAndPreRoll()
        {
            gameManager.gameState = GameState.WaitingForTurn;
            gameManager.turnPhase = TurnPhase.None;

            gameManager.CompleteGameInit();

            Assert.AreEqual(GameState.PlayerTurn, gameManager.gameState);
            Assert.AreEqual(TurnPhase.PreRoll, gameManager.turnPhase);
        }

        [Test]
        public void RollDiceRequest_FromPreRoll_MovesToRollingDice()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.PreRoll;

            gameManager.OnRollDiceRequest(true);

            Assert.AreEqual(TurnPhase.RollingDice, gameManager.turnPhase);
        }

        [Test]
        public void RollDiceRequest_WhenNotInPlayerTurn_DoesNotChangePhase()
        {
            gameManager.gameState = GameState.WaitingForTurn;
            gameManager.turnPhase = TurnPhase.PreRoll;

            LogAssert.Expect(
            LogType.Error,
            CreateRegexLogPattern(
                "Error",
                "Gameplay",
                "GameManager.TryChangeTurnPhase",
                "Illegal action, game state must be PlayerTurn to proceed\\. Current state: WaitingForTurn")
            );
            gameManager.OnRollDiceRequest(true);

            Assert.AreEqual(TurnPhase.PreRoll, gameManager.turnPhase);
        }

        [Test]
        public void DiceRolled_FromRollingDice_MovesToMovingPiece()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.RollingDice;

            var diceEvent = new DiceRolledEvent(2, 3, 5);
            SetPrivate("movementStrategy", new StandardMovementStrategy());
            gameManager.DiceRolled(diceEvent);

            Assert.AreEqual(TurnPhase.MovingPiece, gameManager.turnPhase);
        }

        [Test]
        public void PieceMoveCompleted_FromMovingPiece_MovesToResolvingSpace()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.MovingPiece;

            gameManager.pieceMoveCompletedChannel.Subscribe(gameManager.PieceMoveCompleted);
            gameManager.pieceMoveCompletedChannel.RaiseEvent(true);

            Assert.AreEqual(TurnPhase.ResolvingSpace, gameManager.turnPhase);
        }

        [Test]
        public void PieceMoveCompleted_WithFalseFlag_DoesNotAdvancePhase()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.MovingPiece;

            gameManager.PieceMoveCompleted(false);

            Assert.AreEqual(TurnPhase.MovingPiece, gameManager.turnPhase);
        }

        [Test]
        public void CardDrawn_FromResolvingSpace_MovesToResolvingCards()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.ResolvingSpace;

            var card = ScriptableObject.CreateInstance<Card>();
            var player = ScriptableObject.CreateInstance<Player>();
            var deck = ScriptableObject.CreateInstance<CardDeck>();

            gameManager.OnCardDrawnEvent(card, player, deck);

            Assert.AreEqual(TurnPhase.ResolvingCards, gameManager.turnPhase);
        }

        [Test]
        public void TurnEndedEvent_WithIllegalPhase_DoesNotAdvance()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.RollingDice;

            SetPrivate("playerCount", 2);

            gameManager.OnTurnEndedEvent(true);

            Assert.AreEqual(TurnPhase.RollingDice, gameManager.turnPhase);
        }

        [Test]
        public void TurnEndedEvent_WithFalseFlag_DoesNotAdvance()
        {
            gameManager.gameState = GameState.PlayerTurn;
            gameManager.turnPhase = TurnPhase.PostTurn;

            SetPrivate("playerCount", 2);

            gameManager.OnTurnEndedEvent(false);

            Assert.AreEqual(TurnPhase.PostTurn, gameManager.turnPhase);
        }
    }
}
