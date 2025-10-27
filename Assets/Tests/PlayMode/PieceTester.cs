using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PieceTester
{
    [UnityTest]
    public IEnumerator PieceMovesRight_WhenMoveToCalled()
    {
#if UNITY_EDITOR
        var playerPiecePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPiece.prefab");
        Assert.IsNotNull(playerPiecePrefab, "Failed loading prefab at path: Assets/Prefabs/PlayerPiece.prefab");

        var pieceObj = Object.Instantiate(playerPiecePrefab);
        var piece = pieceObj.GetComponent<Piece>();
        Assert.IsNotNull(piece, "Piece component not on prefab");

        Vector3 startPos = piece.transform.position;
        Vector3 target = startPos + new Vector3(3f, 0f, 0f);

        piece.MoveTo(target);

        //wait for movement to finish
        float timeout = 5f;
        while (Vector3.Distance(piece.transform.position, target) > 0.05f && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Assert
        float movedDistance = Vector3.Distance(startPos, piece.transform.position);
        Assert.That(movedDistance, Is.GreaterThan(0.5f),
            $"Piece did not move far enough. It only moved {movedDistance} units.");


        Object.DestroyImmediate(pieceObj);
#else
        Assert.Ignore("This test can only run in the Unity Editor (AssetDatabase not available in builds).");
        yield break;
#endif
    }
}
