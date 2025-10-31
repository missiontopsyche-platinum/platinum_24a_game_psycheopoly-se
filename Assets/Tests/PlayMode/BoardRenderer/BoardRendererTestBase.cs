using System.Collections;
using NUnit.Framework;
using PsycheOpoly.Board;
using UnityEngine;
using Space = PsycheOpoly.Board.Space;

namespace Tests.PlayMode.BoardRenderer
{
    public class BoardRendererTestBase
    {
        // components and objects
        protected GameObject boardGameObject;
        protected global::BoardRenderer boardRenderer;
        protected Camera testCamera;
        protected GameObject spaceRendererPrefab;
        protected GameObject playerPiecePrefab;

        // event channels
        protected PlayerEventChannel testPlayerEventChannel;
        protected PlayerMovedEventChannel testMoveEventChannel;

        [SetUp]
        public void SetUp()
        {
            // set up camera
            GameObject cameraObject = new GameObject("TestCamera");
            testCamera = cameraObject.AddComponent<Camera>();
            testCamera.orthographicSize = 5.5f;
            testCamera.orthographic = true;
            
            // create boardrenderer
            boardGameObject = new GameObject("TestBoardRenderer");
            boardRenderer = boardGameObject.AddComponent<global::BoardRenderer>();
            
            // create "space prefab" stand-in
            spaceRendererPrefab = new GameObject("SpacePrefab");
            spaceRendererPrefab.AddComponent<BoxCollider>(); // required component of SpaceRenderer
            SpaceRenderer spaceRenderer = spaceRendererPrefab.AddComponent<SpaceRenderer>();
            spaceRenderer.meshRenderer = spaceRendererPrefab.AddComponent<MeshRenderer>();
            
            // create 'player piece' prefab stand-in
            playerPiecePrefab = new GameObject("PiecePrefab");
            Piece piece = playerPiecePrefab.AddComponent<Piece>();
            piece.meshRenderer = playerPiecePrefab.AddComponent<MeshRenderer>();
            piece.meshRenderer.material = new Material(Shader.Find("Unlit/Color"));
            
            // create event channels, ensure subscription
            testPlayerEventChannel = ScriptableObject.CreateInstance<PlayerEventChannel>();
            testPlayerEventChannel.Subscribe(boardRenderer.AddPlayerPiece);
            testMoveEventChannel = ScriptableObject.CreateInstance<PlayerMovedEventChannel>();
            testMoveEventChannel.Subscribe(boardRenderer.MovePiece);

            boardRenderer.mainCamera = testCamera;
            boardRenderer.spaceRendererPrefab = spaceRendererPrefab;
            boardRenderer.playerPiecePrefab = playerPiecePrefab;
            boardRenderer.playerAddedChannel = testPlayerEventChannel;
            
            // generate a test board
            Space[] testSpaces = CreateTestSpaces(40);
            boardRenderer.GenerateBoard(testSpaces);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(boardGameObject);
            Object.Destroy(testCamera.gameObject);
            Object.Destroy(spaceRendererPrefab);
            Object.Destroy(testPlayerEventChannel);
        }

        private Space[] CreateTestSpaces(int count)
        {
            Space[] spaces = new Space[count];
            for (int i = 0; i < count; i++)
            {
                spaces[i] = new PropertySpace($"Space_{i}");
            }
            return spaces;
        }

        /// <summary>
        /// Creates a test player object
        /// </summary>
        /// <param name="id">id of test player</param>
        /// <param name="name">name of test player</param>
        /// <param name="color">color of test player</param>
        /// <returns>Instantiated Player ScriptableObject instance for testing</returns>
        protected Player CreateTestPlayer(int id, string name, Color color)
        {
            Player newPlayer = ScriptableObject.CreateInstance<Player>();
            
            newPlayer.SetId(id);
            newPlayer.SetPName(name);
            newPlayer.SetColor(color);
            
            return newPlayer;
        }

        /// <summary>
        /// Raises PlayerAddedEvent channel and waits shortly for response.
        /// </summary>
        /// <param name="player">Player ScriptableObject instance</param>
        /// <returns></returns>
        protected IEnumerator AddPlayerAndWait(Player player)
        {
            testPlayerEventChannel.RaiseEvent(player);
            yield return new WaitForSeconds(0.25f);
        }
        
        /// <summary>
        /// Moves a piece and waits a set amount of time to complete.
        /// </summary>
        /// <param name="playerId">player id to move</param>
        /// <param name="targetSpace">target space to move to</param>
        /// <param name="waitTime">default: 2 seconds</param>
        /// <returns></returns>
        protected IEnumerator MovePieceAndWait(int playerId, int targetSpace, float waitTime = 2f)
        {
            boardRenderer.MovePiece(new PlayerMovedEvent(playerId, 0, targetSpace));
            yield return new WaitForSeconds(waitTime);
        }
    }
}
