using UnityEngine;
using System.Collections;

public class Piece : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    public void MoveTo(Vector3 targetPosition)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPosition));
    }

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
