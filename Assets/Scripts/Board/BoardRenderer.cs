using System.Collections.Generic;
using System.Linq;
using Logging;
using UnityEngine;
using Assets.Scripts.Events.EventChannelTypes;

public class BoardRenderer : MonoBehaviour
{
    [Header("GameObject References")]
    // camera is used to dynamically create the board in 3d space regardless of the size
    [SerializeField] public Camera mainCamera = new();
    [Header("Prefab Instance References")]
    [SerializeField] public GameObject spaceRendererPrefab;
    [SerializeField] public GameObject playerPiecePrefab;

    [Header("Event Channels")] 
    [SerializeField] public PlayerEventChannel playerAddedChannel;
    [SerializeField] private MortgageFinishedEventChannel mortgageFinishedEventChannel;

    [SerializeField] public PlayerMovedEventChannel playerMovedEventChannel;

    public SpaceRenderer[] spaceRenderers;
    public List<Piece> playerPieces = new();
    private int sideSpacesCount = 11; // number of spaces per side of the board. Can make this dynamic later
    private int edgeBranch = 5;
    private float increment;
    private readonly Dictionary<SpaceData, SpaceRenderer> rendererBySpaceData = new ();

    /// <summary>
    /// Corner targets for piece bumping on shared spaces, normalized
    /// </summary>
    private readonly Vector3[] cornerTargets = {
        new(-1, 1, 0),
        new(1, 1, 0),
        new(-1, -1, 0),
        new(1, -1, 0)
    };
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAddedChannel?.Subscribe(AddPlayerPiece);
        playerMovedEventChannel?.Subscribe(MovePiece);
    }

    private void OnDestroy()
    {
        ClearBoard();
        playerAddedChannel.Unsubscribe(AddPlayerPiece);
        playerMovedEventChannel?.Unsubscribe(MovePiece);
    }

    public void GenerateBoard(SpaceData[] spaces)
    {
        // only allow board generation in play mode
        if (!Application.isPlaying)
        {
            Logging.Logger.Warn("GenerateBoard",
                "Board generation attempted in EditMode - skipping.",
                LogCategory.UI,
                this);
            return;
        }
        
        ClearBoard();
        rendererBySpaceData.Clear();

        if (spaces == null || spaces.Length == 0)
        {
            Logging.Logger.Warn("GenerateBoard", 
                "No spaces provided to BoardRenderer", 
                LogCategory.UI, 
                this);
            return;
        }
        
        // set side spaces count based on incoming data
        // Remove the corners (-4), divide by 4 to get side length without corners. +2 to re-add adjoining corners.
        sideSpacesCount = ((spaces.Length - 4) / 4) + 2;
        edgeBranch = (sideSpacesCount - 1) / 2; // this assumes the side spaces are odd... we basically know they are already.
        
        // verify that the board has enough spaces
        int expectedSpaces = 4 * (sideSpacesCount - 1);
        if (spaces.Length != expectedSpaces)
        {
            Logging.Logger.Warn("GenerateBoard", 
                $"Space count {spaces.Length} doesn't form a perfect square board. Expected {expectedSpaces}.", 
                LogCategory.UI, 
                this);
            return;
        }

        spaceRenderers = new SpaceRenderer[spaces.Length];
        
        // vertical units of space in ortho-camera view from origin
        float size = mainCamera.orthographicSize; 
        // distance in units between spaces, also space scale value.
        increment = (size * 2) / sideSpacesCount;

        for (int i = 0; i < spaces.Length; i++)
        {
            Vector2 position = GetSpacePosition(i, increment);
            SpaceRenderer newRenderer = InstantiateSpace(
                position.x, position.y, spaces[i], increment);
            spaceRenderers[i] = newRenderer;
            rendererBySpaceData[spaces[i]] = newRenderer;
        }
        
        OnBoardReady();
    }

    private Vector2 GetSpacePosition(int index, float increment)
    {
        float halfBoard = edgeBranch * increment;
        int spacesPerSide = sideSpacesCount - 1; // corners shared between sides
        
        // determine which side of board we're on (0 = bot, 1 = left, 2 = top, 3 = right)
        int sideIndex = index / spacesPerSide;
        int positionOnSide = index % spacesPerSide;
        
        return sideIndex switch
        {
            0 => new Vector2(halfBoard - (positionOnSide * increment), -halfBoard),  // bot -> left
            1 => new Vector2(-halfBoard, -halfBoard + (positionOnSide * increment)), // left -> top
            2 => new Vector2(-halfBoard + (positionOnSide * increment), halfBoard),  // top -> right
            3 => new Vector2(halfBoard, halfBoard - (positionOnSide * increment)),   // right -> bot
            _ => Vector2.zero
        };
    }

    private SpaceRenderer InstantiateSpace(float x, float y, SpaceData spaceData, float scale)
    {
        GameObject newSpace = Instantiate(spaceRendererPrefab, transform);
        SpaceRenderer newRenderer = newSpace.GetComponent<SpaceRenderer>();
        newSpace.transform.position = new Vector3(x, y, 0);
        newRenderer.SetUpSpace(spaceData, scale);
        return newRenderer;
    }

    private void OnBoardReady()
    {
        // Fire event here for board being ready
        Logging.Logger.Info("OnBoardReady", 
            "Board generation complete.", 
            LogCategory.UI, 
            this);
    }

    private void ClearBoard()
    {
        if (spaceRenderers != null)
        {
            foreach (var spaceRenderer in spaceRenderers)
                if (spaceRenderer != null)
                {
                    if (Application.isPlaying)   
                        Destroy(spaceRenderer.gameObject);
                    else
                        DestroyImmediate(spaceRenderer.gameObject);
                }
        }

        spaceRenderers = null;
    }

    /// <summary>
    /// Called when Player is added to initialize a piece for them with their color, name, and id.
    /// </summary>
    /// <param name="player">Player information to add to piece</param>
    public void AddPlayerPiece(Player player)
    {
        // create and initialize new piece object
        GameObject newPlayer = Instantiate(playerPiecePrefab);
        Piece piece = newPlayer.GetComponent<Piece>();
        piece.InitializePiece(player.GetId(), player.GetPName(), player.GetColor(), player.GetPiecePrefab());
        
        // scale new player game object to board scale
        newPlayer.transform.localScale *= increment;
        
        // add to pieces list and sort by ID (just in case they're "added" in the wrong order
        playerPieces.Add(piece);
        playerPieces = playerPieces.OrderBy(playerPiece => playerPiece.playerId).ToList();
        
        // move the piece to the starting location (GO)
        MovePiece(new PlayerMovedEvent(piece.playerId, 0, 0, null));
    }

    /// <summary>
    /// Move piece on the board in world space. Takes in playerID and the target space index. Bumps pieces if
    /// the space is crowded with more than 1 piece.
    /// </summary>
    /// <param name="playerId">id of the moving player</param>
    /// <param name="targetSpaceIndex">space index to move to</param>
    public void MovePiece(PlayerMovedEvent mpe)
    {
        // this is extremely simple for now, we can expand this to be board-aware and stick to the edges of the
        // screen, but for now this is good enough for a demo.
        
        // find piece by id
        Piece movingPiece = playerPieces.Find(piece => piece.playerId == mpe.id);
        // null check
        if (movingPiece == null)
        {
            Debug.LogWarning($"No piece found with playerId {mpe.id}");
            return;
        }
        
        Vector3 targetPosition = spaceRenderers[mpe.newPosition].transform.position;
        movingPiece.spaceIndex = mpe.newPosition;
        Vector3 bumpPosition = BumpCrowdedSpacePieces(mpe.newPosition, mpe.id);
        Vector3[] vSteps = BuildVectorPathFromIndexPath(mpe.pathIndices);
        
        if (bumpPosition == Vector3.zero)
            movingPiece.MoveToWaypoints(vSteps);
        else
        {
            vSteps[^1] = bumpPosition; // ^1 means get the last element... C# 8.0 feature. equivalent to ".Length - x".
            movingPiece.MoveToWaypoints(vSteps);
        }
    }

    private Vector3[] BuildVectorPathFromIndexPath(int[] path)
    {
        if (path.Length < 1)
            return new [] { Vector3.zero };
        
        Vector3[] vPath = new Vector3[path.Length];
        
        for (int i = 0; i < path.Length; i++)
        {
            Vector3 vStep = spaceRenderers[path[i]].transform.position;
            vPath[i] = vStep;
        }

        return vPath;
    }

    /// <summary>
    /// Checks the space for multiple pieces. If there is more than 1 piece on a space, it will bump those
    /// pieces to the edges to make room for each one.
    /// </summary>
    /// <param name="targetSpaceIndex">space index we're checking for bump</param>
    private Vector3 BumpCrowdedSpacePieces(int targetSpaceIndex, int currentPlayerId)
    {
        Vector3 rawSpacePosition = spaceRenderers[targetSpaceIndex].transform.position;
        List<Piece> piecesOnTarget = new List<Piece>();
        Vector3 currentPlayerTargetBump = Vector3.zero;

        foreach (Piece piece in playerPieces)
        {
            if (piece.spaceIndex == targetSpaceIndex)
                piecesOnTarget.Add(piece);
        }

        if (piecesOnTarget.Count < 2) // no bump
            return currentPlayerTargetBump;
        
        // bump pieces
        for (int i = 0; i < piecesOnTarget.Count; i++)
        {
            // offset position by corner normal * 1/4 increment amount (slightly to the corner)
            Vector3 targetPosition = rawSpacePosition + (cornerTargets[i] * (increment / 4f));
            if (piecesOnTarget[i].playerId == currentPlayerId)
                currentPlayerTargetBump = targetPosition;
            else
                piecesOnTarget[i].MoveTo(targetPosition, true); // this shuffles existing pieces into a new bump pos
        }

        return currentPlayerTargetBump;
    }

    /// <summary>
    /// This is a helper method for testing.
    /// While using the test scene the player manager
    /// automatically fills everything forcing adding plays to tests
    /// to fail. This is run on every test to fix it.
    /// </summary>
    /// <returns></returns>
    public bool ClearPlayers()
    {
        playerPieces.Clear();

        if (playerPieces.Count == 0)
            return true;

        return false;
    }
}
