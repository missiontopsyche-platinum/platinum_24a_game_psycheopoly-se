using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace Assets.Tests.EditMode
{

    public class GameManager_Initialize_Test
    {
        GameObject go;
        GameManager gm;

        [SetUp]
        public void Setup()
        {
            go = new GameObject("GM_Init_Test");

            gm = go.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null)
            {
                UObject.DestroyImmediate(go);

            }
        }

        [Test]
        public void Initialize_SetsState_ToInitializing()
        {
            Assert.AreEqual(GameState.None, gm.State);
            gm.Initialize();
            Assert.AreEqual(GameState.Initializing, gm.State);
        }
    }
}
