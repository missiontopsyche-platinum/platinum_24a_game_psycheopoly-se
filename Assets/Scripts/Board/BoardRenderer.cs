using System.Collections.Generic;
using Logging;
using PsycheOpoly.Board;
using UnityEngine;
using UnityEngine.Serialization;
using Space = PsycheOpoly.Board.Space;

public class BoardRenderer : MonoBehaviour
{
    // camera is used to dynamically create the board in 3d space regardless of the size
    [SerializeField] private Camera mainCamera = new();
    [SerializeField] private GameObject spaceRendererPrefab;

    private SpaceRenderer[] spaceRenderers;
    private int sideSpacesCount = 11; // number of spaces per side of the board. Can make this dynamic later
    private int edgeBranch = 5;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy() => ClearBoard();

    public void GenerateBoard(Space[] spaces)
    {
        // only allow board generation in play mode
        if (!Application.isPlaying)
        {
            Logging.Logger.Warn("GenerateBoard",
                "Board generation attemped in EditMode - skipping.",
                LogCategory.UI,
                this);
            return;
        }
        
        ClearBoard();

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
        float increment = (size * 2) / sideSpacesCount;

        for (int i = 0; i < spaces.Length; i++)
        {
            Vector2 position = GetSpacePosition(i, increment);
            SpaceRenderer newRenderer = InstantiateSpace(
                position.x, position.y, spaces[i], increment);
            spaceRenderers[i] = newRenderer;
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

    private SpaceRenderer InstantiateSpace(float x, float y, Space spaceData, float scale)
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
}
