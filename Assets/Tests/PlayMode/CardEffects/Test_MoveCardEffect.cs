using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
namespace Assets.Tests.PlayMode.CardEffects
{
    public class Test_MoveCardEffect : CardEffectTestBase
    {
        [UnityTest]
        public IEnumerator MoveCardEffect_MovesForward_RaisesCorrectEvent()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<MoveCardEffect>();
            effect.Type = MoveCardEffect.EffectType.MoveForward;
            effect.SpacesToMove = 4;

            var moveChannel = CreateChannel<MovePlayerEventChannel>();
            var noActionChannel = CreateChannel<NoActionLandingEventChannel>();

            MovePlayerEvent capturedEvent = null;
            moveChannel.Subscribe(evt => capturedEvent = evt);

            effect.MovePlayerEventChannel = moveChannel;
            effect.NoActionLandingEventChannel = noActionChannel;

            effect.ApplyEffect(player);
            yield return null;

            Assert.IsNotNull(capturedEvent, "MoveCardEffect should raise a MovePlayerEvent.");
            Assert.AreEqual(0, capturedEvent.id, "Event should target the correct player id.");
            Assert.AreEqual(4, capturedEvent.spacesToMove, "Event should move the player forward 4 spaces.");
        }

        [UnityTest]
        public IEnumerator MoveCardEffect_MovesBackward_RaisesNegativeEvent()
        {
            var player = CreatePlayer(1, 1500);

            var effect = CreateEffect<MoveCardEffect>();
            effect.Type = MoveCardEffect.EffectType.MoveBackward;
            effect.SpacesToMove = 3;

            var moveChannel = CreateChannel<MovePlayerEventChannel>();
            var noActionChannel = CreateChannel<NoActionLandingEventChannel>();

            MovePlayerEvent capturedEvent = null;
            moveChannel.Subscribe(evt => capturedEvent = evt);

            effect.MovePlayerEventChannel = moveChannel;
            effect.NoActionLandingEventChannel = noActionChannel;

            effect.ApplyEffect(player);
            yield return null;

            Assert.IsNotNull(capturedEvent, "MoveCardEffect should raise a MovePlayerEvent.");
            Assert.AreEqual(1, capturedEvent.id, "Event should target the correct player id.");
            Assert.AreEqual(-3, capturedEvent.spacesToMove, "Backward movement should be negative.");
        }
    }
}
