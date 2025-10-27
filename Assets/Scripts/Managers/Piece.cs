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

    private MeshRenderer meshRenderer;
    public int spaceIndex { get; set; } 
    
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = pieceColor;
        spaceIndex = 0;
    }

    public void InitializePiece(String name, Color color)
    {
        this.name = name;
        pieceColor = color;
        meshRenderer.material.color = pieceColor;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPosition));
    }

    /// <summary>
    /// moves the piece toward a specified target; this will need to be adjusted
    /// in the future to adjust the target to a new space on the board.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerator MoveRoutine(Vector3 target)
    {
        //restructure how time elapses to test for runnabilty
        //assume 50 fps if Time.deltaTime == 0
        float simulatedDelta = Time.deltaTime > 0 ? Time.deltaTime : 0.02f; 

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * simulatedDelta
            );
            yield return null;
        }

        transform.position = target;
    }


}
