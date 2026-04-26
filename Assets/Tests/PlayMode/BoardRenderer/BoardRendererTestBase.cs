using Logging;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using BoardManager = PsycheOpoly.Board.BoardManager; 
using Logger = Logging.Logger;

namespace Tests.PlayMode.BoardRenderer
{
    public class BoardRendererTestBase : PlayTestBase
    {
        // components and objects
        protected GameObject boardGameObject;
        protected global::BoardRenderer boardRenderer;
        protected BoardManager boardManager;
        protected GameManager gameManager;
        protected Camera testCamera;
        protected GameObject spaceRendererPrefab;
        protected GameObject playerPiecePrefab;
        
        // event channels
        protected PlayerEventChannel testPlayerEventChannel;
        protected PlayerMovedEventChannel testMoveEventChannel;
        
        [SetUp]
        public void SetUp()
        {
            Logging.Logger.Trace("DicePanelControllerPlayModeTests.SetUp",
                "Setting up DicePanelController PlayMode test",
                Logging.LogCategory.UI,
                this);

            //Create an on sceneLoaded event handler to build objects
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("PlayTestScene", LoadSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.UnloadSceneAsync("PlayTestScene");
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
            bool playerAdded = false;

            testPlayerEventChannel.Subscribe((v) => playerAdded = true);

            testPlayerEventChannel.RaiseEvent(player);

            yield return new WaitUntil(() => playerAdded);
        }
                
        /// <summary>
        /// Moves a piece and waits a set amount of time to complete.
        /// </summary>
        /// <param name="playerId">player id to move</param>
        /// <param name="targetSpace">target space to move to</param>
        /// <param name="waitTime">default: 2 seconds</param>
        /// <returns></returns>
        protected IEnumerator MovePieceAndWait(int playerId, int targetSpace)
        {
            Assert.IsNotNull(boardRenderer, "boardRenderer is null before moving piece.");

            testMoveEventChannel.RaiseEvent(new PlayerMovedEvent(playerId, 0, targetSpace, null));

            yield return new WaitForSeconds(0.75f);
        }
        
        /// <summary>
        /// Event handler for "SceneLoaded" event to build out testing objects after
        /// PlayTestScene has loaded. This is necessary to ensure the scene is loaded
        /// Before trying to access information
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
                Assert.IsNotNull(boardGameObject, "Wh" +
                    "y is this fucking null?");
            }
            
            boardManager = boardGameObject.GetComponent<BoardManager>() as BoardManager;
            boardRenderer = boardManager.boardRenderer;
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            foreach (var panel in GameObject.FindObjectsByType<PlayerPanelController>(
                        FindObjectsInactive.Include,
                        FindObjectsSortMode.None))
            {
                panel.enabled = false;
            }

            testPlayerEventChannel = boardManager.playerAddedChannel;
            testPlayerEventChannel.Subscribe(boardRenderer.AddPlayerPiece);
            testMoveEventChannel = boardManager.playerMovedChannel;
            // generate a test board
            SpaceData[] testSpaces = CreateTestSpaces(40);
            boardRenderer.GenerateBoard(testSpaces);

            //Ensures any players auto made by the playermanager are cleared so adding player tests
            //function correctly
            boardManager.ClearPlayers();
            boardRenderer.ClearPlayers();

            sceneLoaded = true;
        }
    }
}
