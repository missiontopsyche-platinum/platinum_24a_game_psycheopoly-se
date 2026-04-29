using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.CardTests
{
    public class MoveCardEffectTest : CardEffectBaseTest
    {
        [Test]
        public void MoveCardEffect_Forward_RaisesPositiveMove()
        {
            testPlayer.SetId(0);

            MovePlayerEventChannel moveChannel = CreateChannel<MovePlayerEventChannel>();
            NoActionLandingEventChannel noActionChannel = CreateChannel<NoActionLandingEventChannel>();

            List<MovePlayerEvent> raised = new();
            moveChannel.Subscribe(e => raised.Add(e));

            var effect = TrackEffect(ScriptableObject.CreateInstance<MoveCardEffect>());
            effect.Type = MoveCardEffect.EffectType.MoveForward;
            effect.SpacesToMove = 5;
            effect.MovePlayerEventChannel = moveChannel;
            effect.NoActionLandingEventChannel = noActionChannel;

            effect.ApplyEffect(testPlayer);

            Assert.AreEqual(1, raised.Count);
            Assert.AreEqual(testPlayer.GetId(), raised[0].id);
            Assert.AreEqual(5, raised[0].spacesToMove);
        }

        [Test]
        public void MoveCardEffect_Backward_RaisesNegativeMove()
        {
            testPlayer.SetId(0);

            MovePlayerEventChannel moveChannel = CreateChannel<MovePlayerEventChannel>();
            NoActionLandingEventChannel noActionChannel = CreateChannel<NoActionLandingEventChannel>();

            List<MovePlayerEvent> raised = new();
            moveChannel.Subscribe(e => raised.Add(e));

            var effect = TrackEffect(ScriptableObject.CreateInstance<MoveCardEffect>());
            effect.Type = MoveCardEffect.EffectType.MoveBackward;
            effect.SpacesToMove = 3;
            effect.MovePlayerEventChannel = moveChannel;
            effect.NoActionLandingEventChannel = noActionChannel;

            effect.ApplyEffect(testPlayer);

            Assert.AreEqual(1, raised.Count);
            Assert.AreEqual(testPlayer.GetId(), raised[0].id);
            Assert.AreEqual(-3, raised[0].spacesToMove);
        }
    }
}