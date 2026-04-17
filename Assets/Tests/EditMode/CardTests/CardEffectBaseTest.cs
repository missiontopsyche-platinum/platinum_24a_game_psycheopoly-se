using Assets.Scripts.Events.EventChannelTypes;
using NUnit.Framework;
using PsycheOpoly.Board;
using Tests.EditMode;
using UnityEngine;

namespace Tests.EditMode.CardTests
{
    public class CardEffectBaseTest : ManagerTestBase
    {
        protected Player testPlayer;
        private CardEffect trackedEffect;

        [SetUp]
        protected void SetUp()
        {
            InitializeTestLogger();

            testPlayer = ScriptableObject.CreateInstance<Player>();
            testPlayer.SetPName("Test Player");
            testPlayer.SetMoney(1500);
        }

        protected T TrackEffect<T>(T effect) where T : CardEffect
        {
            trackedEffect = effect;
            return effect;
        }

        [TearDown]
        public void TearDown()
        {
            DestroyTestObjects(testPlayer, trackedEffect);
        }
    }
}


