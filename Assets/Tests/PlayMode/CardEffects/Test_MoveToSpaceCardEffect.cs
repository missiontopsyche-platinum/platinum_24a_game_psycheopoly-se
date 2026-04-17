using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;

namespace Assets.Tests.PlayMode.CardEffects
{
    public class Test_MoveToSpaceCardEffect : CardEffectTestBase
    {
        [UnityTest]
        public IEnumerator MoveToSpaceCardEffect_RaisesCorrectTarget()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<MoveToSpaceCardEffect>();
            effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace;


            var channel = CreateChannel<MoveToSpaceEventChannel>();
            MoveToSpaceEvent captured = null;
            channel.Subscribe(evt => captured = evt);
            effect.moveToSpaceEventChannel = channel;

            effect.ApplyEffect(player);
            yield return null;

            Assert.NotNull(captured);
            Assert.AreEqual(player, captured.player);
            Assert.AreEqual(MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace, captured.targetKind);
        }
    }
}
