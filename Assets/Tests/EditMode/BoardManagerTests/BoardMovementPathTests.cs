using NUnit.Framework;
using PsycheOpoly.Board;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Tests.EditMode.BoardManagerTests
{
    /// <summary>
    /// US-253 Movement Rules:
    ///  - PlayerMovedEvent must contain from/to/path
    ///  - movement must wrap correctly, handle negative steps
    ///  and raise passedGoChannel when wrapping forward
    /// </summary>
    public class BoardMovementPathTests : BoardManagerTestBase
    {
        private int lastPassedGoPlayer = -1;
        private PlayerMovedEvent lastMoveEvent;

        [SetUp]
        public void SetUp()
        {
            boardManager.InitializeBoard();

            lastPassedGoPlayer = -1;
            lastMoveEvent = null;

            boardManager.playerMovedChannel.Subscribe(evt =>
            {
                lastMoveEvent = evt;
            });

            //pass go
            boardManager.passedGoChannel.Subscribe(pid =>
            {
                lastPassedGoPlayer = pid;
            });
        }

        [Test]
        public void MoveForward_NoWrap_ProducesCorrectPath()
        {
            boardManager.SetPlayerPosition(1, 2); 

            boardManager.MovePlayer(new MovePlayerEvent(1, 3));

            Assert.AreEqual(5, boardManager.GetPlayerPosition(1), "Final position wrong");
            Assert.NotNull(lastMoveEvent, "Movement event did not fire");

            CollectionAssert.AreEqual(
                new[] { 3, 4, 5 },
                lastMoveEvent.pathIndices,
                "Path indices incorrect"
            );
        }
        
        [Test]
        public void MoveForward_WrapsAroundBoard_ProducesCorrectPath()
        {
            boardManager.SetPlayerPosition(1, 39);

            boardManager.MovePlayer(new MovePlayerEvent(1, 3));

            Assert.AreEqual(2, boardManager.GetPlayerPosition(1), "Wrap final position incorrect");
            Assert.NotNull(lastMoveEvent);



            CollectionAssert.AreEqual(
                new[] { 0, 1, 2 },
                lastMoveEvent.pathIndices,
                "Wrap path incorrect"

            );

            Assert.AreEqual(1, lastPassedGoPlayer, "PassedGo should fire when wrapping forward");
        }

        [Test]
        public void MoveBackward_ProducesCorrectReversePath()
        {
            boardManager.SetPlayerPosition(1, 4);

            boardManager.MovePlayer(new MovePlayerEvent(1, -2));

            Assert.AreEqual(2, boardManager.GetPlayerPosition(1));
            CollectionAssert.AreEqual(
                new[] { 3, 2 },
                lastMoveEvent.pathIndices,
                "Reverse path incorrect"
            );

            Assert.AreEqual(-1, lastPassedGoPlayer, "Backward moves should NOT trigger passedGo");
        }

        [Test]
        public void MoveBackward_WrapsCorrectly()
        {
            boardManager.SetPlayerPosition(1, 0);

            boardManager.MovePlayer(new MovePlayerEvent(1, -3));

            Assert.AreEqual(37, boardManager.GetPlayerPosition(1), "Backward wrap incorrect");
            CollectionAssert.AreEqual(
                new[] { 39, 38, 37 },
                lastMoveEvent.pathIndices,
                "Backward wrap path incorrect"
            );
        }

        [Test]
        public void MoveZero_ProducesEmptyPath()
        {
            boardManager.SetPlayerPosition(1, 3);

            boardManager.MovePlayer(new MovePlayerEvent(1, 0));

            Assert.AreEqual(3, boardManager.GetPlayerPosition(1));
            Assert.NotNull(lastMoveEvent);

            Assert.AreEqual(0, lastMoveEvent.pathIndices.Length, "Path should be empty for 0 movement");
        }

        [Test]
        public void PlayerMovedEvent_ContainsCorrectFrom_To_And_Path()
        {
            boardManager.SetPlayerPosition(2, 1);

            boardManager.MovePlayer(new MovePlayerEvent(2, 2));

            Assert.AreEqual(1, lastMoveEvent.previousPosition);
            Assert.AreEqual(3, lastMoveEvent.newPosition);
            CollectionAssert.AreEqual(
                new[] { 2, 3 },
                lastMoveEvent.pathIndices,
                "Event path incorrect"
            );
        }
    }
}
