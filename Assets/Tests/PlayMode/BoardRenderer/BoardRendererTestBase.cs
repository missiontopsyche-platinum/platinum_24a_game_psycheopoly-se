using Logging;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BoardManager = PsycheOpoly.Board.BoardManager; 
using Logger = Logging.Logger;

namespace Tests.PlayMode.BoardRenderer
{
    public class BoardRendererTestBase
    {
        // components and objects
        protected GameObject boardGameObject;
        protected global::BoardRenderer boardRenderer;
        protected BoardManager boardManager;
        protected Camera testCamera;
        protected GameObject spaceRendererPrefab;
        protected GameObject playerPiecePrefab;

        // event channels
        protected PlayerEventChannel testPlayerEventChannel;
        protected PlayerMovedEventChannel testMoveEventChannel;

        [SetUp]
        public void SetUp()
        {   
            //Init logger
            Logger.Initialize(LogSettings.Current());
            
            //Create an on sceneLoaded event handler to build objects
            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadScene("PlayTestScene");
            
            
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadScene("PlayTestScene");
            
        }

        private SpaceData[] CreateTestSpaces(int count)
        {
            SpaceData[] spaces = new SpaceData[count];
            for (int i = 0; i < count; i++)
            {
                PropertySpaceData newSpace = ScriptableObject.CreateInstance<PropertySpaceData>();
                newSpace.spaceName = $"Space_{i}";
                spaces[i] = newSpace;
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
            boardRenderer.MovePiece(new PlayerMovedEvent(playerId, 0, targetSpace, null));
            yield return new WaitForSeconds(waitTime);
        }

        protected IEnumerator LoadScenceAsync(string scene)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PlayTestScene"); 

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Event handler for "SceneLoaded" event to build out testing objects after
        /// PlayTestScene has loaded. This is necessary to ensure the scene is loaded
        /// Before trying to access information
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        protected void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // set up camera using Scene Manager
            testCamera = GameObject.FindFirstObjectByType<Camera>();
            if (testCamera != null)
            {
                Logger.Info("BoardManagerTestBase.SetUp",
                "Sucessfully Found the Camera.",
                LogCategory.Core, this);
            }

            // get the board game object
            boardGameObject = GameObject.Find("Board");
            if (boardGameObject == null)
            {
                Logger.Info("BoardManagerTestBase.SetUp",
                "Cannot Locate Board.",
                LogCategory.Core, this);
                Assert.IsNotNull(boardGameObject, "Why is this fucking null?");
            }

            boardManager = boardGameObject.GetComponent<BoardManager>() as BoardManager;
            boardRenderer = boardManager.boardRenderer;

            testPlayerEventChannel = boardManager.playerAddedChannel;
            testPlayerEventChannel.Subscribe(boardRenderer.AddPlayerPiece);
            testMoveEventChannel = boardManager.playerMovedChannel;
            testMoveEventChannel.Subscribe(boardRenderer.MovePiece);

            // generate a test board
            SpaceData[] testSpaces = CreateTestSpaces(40);
            boardRenderer.GenerateBoard(testSpaces);

            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();
        }
    }
}
