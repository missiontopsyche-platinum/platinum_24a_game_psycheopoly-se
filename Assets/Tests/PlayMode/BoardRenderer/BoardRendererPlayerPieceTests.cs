using Logging;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Logger = Logging.Logger;
namespace Tests.PlayMode.BoardRenderer
{
    public class BoardRendererPlayerPieceTests : BoardRendererTestBase
    {
        [UnityTest]
        public IEnumerator AddPlayerPiece_CreatesNewPiece()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Logger.Info("BoardRendererPlayerPieceTests.AddPlayerPiece_CreatesNewPiece", "We're here again", LogCategory.Core, this);
            Player testPlayer = CreateTestPlayer(0, "TestPlayer", Color.blue);

            yield return AddPlayerAndWait(testPlayer);
            
            Assert.AreEqual(1, boardRenderer.playerPieces.Count, "Should have one player piece");
            Assert.AreEqual(0, boardRenderer.playerPieces[0].playerId, "Player ID should match");
            Assert.AreEqual("TestPlayer", boardRenderer.playerPieces[0].name, "Player name should match");
        }

        [UnityTest]
        public IEnumerator AddPlayerPiece_SortsById()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player player1 = CreateTestPlayer(2, "Player2", Color.red);
            Player player2 = CreateTestPlayer(0, "Player0", Color.blue);
            Player player3 = CreateTestPlayer(1, "Player1", Color.green);

            yield return AddPlayerAndWait(player1);
            yield return AddPlayerAndWait(player2);
            yield return AddPlayerAndWait(player3);
            
            Assert.AreEqual(3, boardRenderer.playerPieces.Count, "Should have three player pieces");
            Assert.AreEqual(0, boardRenderer.playerPieces[0].playerId, "First player should have ID 0");
            Assert.AreEqual(1, boardRenderer.playerPieces[1].playerId, "Second player should have ID 1");
            Assert.AreEqual(2, boardRenderer.playerPieces[2].playerId, "Third player should have ID 2");
        }

        [UnityTest]
        public IEnumerator AddPlayerPiece_StartsAtGo()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player testPlayer = CreateTestPlayer(0, "TestPlayer", Color.blue);

            yield return AddPlayerAndWait(testPlayer);
            
            Assert.AreEqual(0, boardRenderer.playerPieces[0].spaceIndex, "Piece should start at space index 0, aka Go");
        }

        [UnityTest]
        public IEnumerator MovePiece_UpdatesSpaceIndex()
        {

            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player testPlayer = CreateTestPlayer(0, "TestPlayer", Color.blue);
            
            yield return AddPlayerAndWait(testPlayer);

            yield return MovePieceAndWait(0, 5);
            
            Assert.AreEqual(5, boardRenderer.playerPieces[0].spaceIndex, "Space index should be updated to 5");
        }

        [UnityTest]
        public IEnumerator MovePiece_MovesPhysically()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player testPlayer = CreateTestPlayer(0, "TestPlayer", Color.blue);
            
            yield return AddPlayerAndWait(testPlayer);

            Vector3 initialPosition = boardRenderer.playerPieces[0].transform.position;
            Vector3 targetSpacePosition = boardRenderer.spaceRenderers[5].transform.position;
            
            yield return MovePieceAndWait(0, 5);
            
            Assert.AreNotEqual(initialPosition, boardRenderer.playerPieces[0].transform.position,
                "Piece should have moved from initial position");
            float distance = Vector3.Distance(boardRenderer.playerPieces[0].transform.position, targetSpacePosition);
            Assert.Less(distance, 0.5f, "Piece should be near target space, with buffer for bump offset");
        }

        [UnityTest]
        public IEnumerator BumpCrowdedSpacePieces_NoBumpForSinglePiece()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player testPlayer = CreateTestPlayer(0, "TestPlayer", Color.blue);
            
            yield return AddPlayerAndWait(testPlayer);

            Vector3 targetSpacePosition = boardRenderer.spaceRenderers[5].transform.position;
            
            yield return MovePieceAndWait(0, 5);

            float distance = Vector3.Distance(boardRenderer.playerPieces[0].transform.position, targetSpacePosition);
            Assert.Less(distance, 0.01f, "Single piece should be at exact space position");
        }

        [UnityTest]
        public IEnumerator BumpCrowdedSpacePieces_BumpsTwoPieces()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player player1 = CreateTestPlayer(0, "Player1", Color.red);
            Player player2 = CreateTestPlayer(1, "Player2", Color.blue);
            yield return AddPlayerAndWait(player1);
            yield return AddPlayerAndWait(player2);
            
            Vector3 spacePosition = boardRenderer.spaceRenderers[5].transform.position;
            
            yield return MovePieceAndWait(0, 5);
            yield return MovePieceAndWait(1, 5);
            
            Vector3 piece1Pos = boardRenderer.playerPieces[0].transform.position;
            Vector3 piece2Pos = boardRenderer.playerPieces[1].transform.position;
            
            Assert.AreNotEqual(spacePosition, piece1Pos, "Piece 1 should be bumped from center");
            Assert.AreNotEqual(spacePosition, piece2Pos, "Piece 2 should be bumped from center");
            Assert.AreNotEqual(piece1Pos, piece2Pos, "Pieces should not overlap");
        }

        [UnityTest]
        public IEnumerator BumpCrowdedSpacePieces_BumpsFourPiecesToCorners()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player player1 = CreateTestPlayer(0, "Player1", Color.red);
            Player player2 = CreateTestPlayer(1, "Player2", Color.blue);
            Player player3 = CreateTestPlayer(2, "Player3", Color.green);
            Player player4 = CreateTestPlayer(3, "Player4", Color.yellow);
            
            yield return AddPlayerAndWait(player1);
            yield return AddPlayerAndWait(player2);
            yield return AddPlayerAndWait(player3);
            yield return AddPlayerAndWait(player4);
            
            Vector3 spacePosition = boardRenderer.spaceRenderers[10].transform.position;
            
            for (int i = 0; i < 4; i++)
            {
                yield return MovePieceAndWait(i, 5);
            }
            yield return new WaitForSeconds(1f); // wait for all pieces to finish movement
            
            // Verify piece is offset in both X and Y 
            for (int i = 0; i < 4; i++)
            {
                Vector3 piecePos = boardRenderer.playerPieces[i].transform.position;
                Assert.AreNotEqual(spacePosition, piecePos, 
                    $"Piece {i} should be bumped from center");
                float xDiff = Mathf.Abs(piecePos.x - spacePosition.x);
                float yDiff = Mathf.Abs(piecePos.y - spacePosition.y);
                Assert.Greater(xDiff, 0.01f, $"Piece {i} should have X offset");
                Assert.Greater(yDiff, 0.01f, $"Piece {i} should have Y offset");
            }
            
            // Verify all pieces are at different positions
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    Assert.AreNotEqual(boardRenderer.playerPieces[i].transform.position, 
                        boardRenderer.playerPieces[j].transform.position,
                        $"Pieces {i} and {j} should not overlap");
                }
            }
        }
        
        [UnityTest]
        public IEnumerator BumpCrowdedSpacePieces_LowerIdGetsFirstCorner()
        {
            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            Player player0 = CreateTestPlayer(0, "Player0", Color.red);
            Player player1 = CreateTestPlayer(1, "Player1", Color.blue);
        
            yield return AddPlayerAndWait(player0);
            yield return AddPlayerAndWait(player1);
            
            Vector3 spacePosition = boardRenderer.spaceRenderers[7].transform.position;
            
            yield return MovePieceAndWait(0, 7);
            yield return MovePieceAndWait(1, 7);

            // player0 gets top-left corner (negative X, positive Y)
            Vector3 player0Pos = boardRenderer.playerPieces[0].transform.position;
            Assert.Less(player0Pos.x, spacePosition.x, "Player 0 should be left of center");
            Assert.Greater(player0Pos.y, spacePosition.y, "Player 0 should be above center");
        }
    }
}
