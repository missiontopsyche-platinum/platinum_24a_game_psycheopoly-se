using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// this represents how the actual piece in the game will move across the board
/// i used the Unity coroutine because it seemed the easiest but check on this!
/// </summary>
public class Piece : MonoBehaviour
{
    [SerializeField] private Color pieceColor = Color.white;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private BooleanEventChannel pieceMoveCompletedEventChannel;

    public MeshRenderer meshRenderer;
    public int playerId;
    public int spaceIndex;
    
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = pieceColor;
        spaceIndex = 0;
    }

    public void InitializePiece(int id, String name, Color color)
    {
        playerId = id;
        this.name = name;
        pieceColor = color;
        if (!meshRenderer)
            meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = pieceColor;
    }

    public void MoveTo(Vector3 targetPosition, bool isBump)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPosition, isBump));
    }

    public void MoveToWaypoints(Vector3[] targetPositions)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPositions));
    }

    /// <summary>
    /// moves the piece toward a specified target; this will need to be adjusted
    /// in the future to adjust the target to a new space on the board.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerator MoveRoutine(Vector3 target, bool isBump)
    {
        //restructure how time elapses to test for runnabilty
        //assume 50 fps if Time.deltaTime == 0
        float simulatedDelta = Time.deltaTime > 0 ? Time.deltaTime : 0.02f; 

        yield return StartCoroutine(MoveTowardsTarget(target, simulatedDelta));

        transform.position = target;
        
        // if this is a piece-correction bump, dont say a turn is complete
        if (!isBump)
            pieceMoveCompletedEventChannel.RaiseEvent(true);
    }
    
    /// <summary>
    /// Moves target over an array of waypoints.
    /// </summary>
    /// <param name="targetPositions">Array of waypoints for the piece to hit.</param>
    /// <returns></returns>
    private IEnumerator MoveRoutine(Vector3[] targetPositions)
    {
        foreach (Vector3 target in targetPositions)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                yield return MoveTowardsTarget(target, Time.deltaTime);
            }

            transform.position = target;
        }
        
        pieceMoveCompletedEventChannel.RaiseEvent(true);
    }

    private IEnumerator MoveTowardsTarget(Vector3 target, float simulatedDelta)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * simulatedDelta
            );
            yield return null;
        }
    }
}
