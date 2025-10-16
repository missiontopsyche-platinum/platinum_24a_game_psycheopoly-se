using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.BoardManagerTests
{
    public class BoardManagerTestBase : ManagerTestBase
    {
        protected GameObject gameObject;
        protected PsycheOpoly.Board.BoardManager boardManager;

        [SetUp]
        public virtual void SetUp()
        {
            gameObject = new GameObject("BoardManagerTests");
            boardManager = gameObject.AddComponent<PsycheOpoly.Board.BoardManager>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            DestroyTestObjects(gameObject);
        }
    }
}
