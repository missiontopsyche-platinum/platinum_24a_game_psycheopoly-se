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

 
    public class GameManager_Transitions_Test
    {
        GameObject go;
        GameManager gm;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("GM_Transition_Test");
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
        public void EndGame_FromNone_IsBlocked()
        {
            //none, should give warning here
            gm.EndGame();
            Assert.AreEqual(GameState.None, gm.State);
        }

        [Test]
        public void Initialize_AllowsTransition_FromNone()
        {

            gm.Initialize();
            Assert.AreEqual(GameState.Initializing, gm.State);
        }
    }
}
