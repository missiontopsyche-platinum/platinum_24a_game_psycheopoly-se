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
    public class GameManager_Events_Test
    {
        GameObject go;
        GameManager gm;



        [SetUp]
        public void SetUp()
        {
            go = new GameObject("GM_Events_Tests");
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
        public void GameStateChanged_Fires_On_Initialize()
        {
            int calls = 0;

            GameState oldS = GameState.None;
            GameState newS = GameState.None;


            gm.GameStateChanged += (o, n) => { calls++; oldS = o; newS = n; };

            gm.Initialize();

            //this is none > initialzing
            Assert.AreEqual(1, calls); 
            Assert.AreEqual(GameState.None, oldS);

            Assert.AreEqual(GameState.Initializing, newS);
        }
    }
}
