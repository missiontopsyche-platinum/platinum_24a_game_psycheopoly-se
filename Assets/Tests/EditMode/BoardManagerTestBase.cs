using NUnit.Framework;
using PsycheOpoly.Board;
using UnityEngine;

public class BoardManagerTestBase : ManagerTestBase
{
    protected GameObject gameObject;
    protected BoardManager boardManager;

    [SetUp]
    public virtual void SetUp()
    {
        gameObject = new GameObject("BoardManagerTests");
        boardManager = gameObject.AddComponent<BoardManager>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        DestroyTestObjects(gameObject);
    }
}
