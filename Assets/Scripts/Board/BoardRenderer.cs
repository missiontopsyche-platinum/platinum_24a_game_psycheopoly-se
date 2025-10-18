using System.Collections.Generic;
using Logging;
using PsycheOpoly.Board;
using UnityEngine;
using UnityEngine.Serialization;

public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    // camera is used to dynamically create the board in 3d space regardless of the size
    [SerializeField] private Camera camera = new();
    [FormerlySerializedAs("spaceRenderer")] [SerializeField] private GameObject spaceRendererPrefab;

    private List<SpaceRenderer> spaceRenderers = new();
    private int sideSpacesCount = 11; // number of spaces per side of the board. Can make this dynamic later
    private int edgeBranch = 5;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // THIS IS TEMPORARY UNTIL WE PASS IN REAL DATA
        List<PsycheOpoly.Board.Space> tempSpaces = new();
        for (int i = 0; i < 40; i++)
            tempSpaces.Add(new PropertySpace($"Test_{i}"));
        
        GenerateBoard(tempSpaces);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        ClearBoard();
    }

    private void GenerateBoard(List<PsycheOpoly.Board.Space> spaces)
    {
        ClearBoard();

        if (spaces == null || spaces.Count == 0)
        {
            Logging.Logger.Warn("GenerateBoard", 
                "No spaces provided to BoardRenderer", 
                LogCategory.UI, 
                this);
            return;
        }
        
        // set side spaces count based on incoming data
        // Remove the corners (-4), divide by 4 to get side length without corners. +2 to re-add adjoining corners.
        sideSpacesCount = ((spaces.Count - 4) / 4) + 2;
        edgeBranch = (sideSpacesCount - 1) / 2; // this assumes the side spaces are odd... we basically know they are already.
        
        // verify that the board has enough spaces
        int expectedSpaces = 4 * (sideSpacesCount - 1);
        if (spaces.Count != expectedSpaces)
        {
            Logging.Logger.Warn("GenerateBoard", 
                $"Space count {spaces.Count} doesn't form a perfect square board. Expected {expectedSpaces}.", 
                LogCategory.UI, 
                this);
            return;
        }
        
        // vertical units of space in ortho-camera view from origin
        float size = camera.orthographicSize; 
        // distance in units between spaces, also space scale value.
        float increment = (size * 2) / sideSpacesCount;

        for (int i = 0; i < spaces.Count; i++)
        {
            Vector2 position = GetSpacePosition(i, increment);
            SpaceRenderer newRenderer = InstantiateSpace(position.x, position.y, increment);
            // TODO pass in scriptable object information to render space properly
            // renderer.Initialize(spaces[i]);
            spaceRenderers.Add(newRenderer);
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

    private SpaceRenderer InstantiateSpace(float x, float y, float scale)
    {
        GameObject newSpace = Instantiate(spaceRendererPrefab, transform);
        SpaceRenderer newRenderer = newSpace.GetComponent<SpaceRenderer>();
        newSpace.transform.position = new Vector3(x, y, 0);
        newRenderer.SetUpSpace(scale);
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
                if(spaceRenderer != null)
                    Destroy(spaceRenderer.gameObject);
            spaceRenderers.Clear();
        }
    }
}
