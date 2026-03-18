using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.EditMode.CardTests
{
    public class PayAndCollectFromAllPlayersTest : CardEffectBaseTest
    {
        [Test]
        public void PayAllPlayers_ChargesInitiator()
        {
            MoneyDistributionEventChannel channel = CreateChannel<MoneyDistributionEventChannel>();
            List<MoneyDistributionEvent> raised = new();
            channel.Subscribe(e => raised.Add(e));

            var effect = TrackEffect(ScriptableObject.CreateInstance<PayAllPlayersCardEffect>());
            effect.Amount = 25;
            effect.payAllPlayersEventChannel = channel;

            effect.ApplyEffect(testPlayer);

            Assert.AreEqual(1, raised.Count);
            Assert.AreSame(testPlayer, raised[0].Player);
            Assert.AreEqual(25, raised[0].Amount);
        }

        [Test]
        public void CollectFromAllPlayers_ChargesOthers()
        {
            MoneyDistributionEventChannel channel = CreateChannel<MoneyDistributionEventChannel>();
            List<MoneyDistributionEvent> raised = new();
            channel.Subscribe(e => raised.Add(e));

            var effect = TrackEffect(ScriptableObject.CreateInstance<CollectFromAllPlayersCardEffect>());
            effect.Amount = 25;
            effect.collectFromAllPlayersEventChannel = channel;

            effect.ApplyEffect(testPlayer);

            Assert.AreEqual(1, raised.Count);
            Assert.AreSame(testPlayer, raised[0].Player);
            Assert.AreEqual(25, raised[0].Amount);
        }
    }
}

