using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Events.EventDataStructures;

namespace Assets.Tests.PlayMode.CardEffects
{
    /// <summary>
    /// PlayMode tests that verify individual CardEffect ScriptableObjects
    /// raise the correct events with the expected payloads.
    /// </summary>
    public class Test_CardEffects : CardEffectTestBase
    {
        // MoveCardEffect

        [Test]
        public void MoveCardEffect_MoveForward_RaisesMovePlayerEvent()
        {
            var player = CreatePlayer(2, 1000);

            var effect = CreateEffect<MoveCardEffect>();
            effect.Type = MoveCardEffect.EffectType.MoveForward;

            effect.SpacesToMove = 4;

            var channel = CreateChannel<MovePlayerEventChannel>();
            MovePlayerEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.MovePlayerEventChannel = channel;

            effect.ApplyEffect(player);

            Assert.NotNull(captured, "MovePlayerEvent should be raised");
            Assert.AreEqual(player.GetId(), captured.id);
            Assert.AreEqual(4, captured.spacesToMove);
        }

        [Test]
        public void MoveCardEffect_MoveBackward_RaisesNegativeMovePlayerEvent()
        {
            var player = CreatePlayer(1, 1000);

            var effect = CreateEffect<MoveCardEffect>();
            effect.Type = MoveCardEffect.EffectType.MoveBackward;

            effect.SpacesToMove = 3;

            var channel = CreateChannel<MovePlayerEventChannel>();
            MovePlayerEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.MovePlayerEventChannel = channel;



            effect.ApplyEffect(player);

            Assert.NotNull(captured, "MovePlayerEvent should be raised");
            Assert.AreEqual(player.GetId(), captured.id);
            Assert.AreEqual(-3, captured.spacesToMove);


        }

        // MoveToSpaceCardEffect
        [Test]
        public void MoveToSpaceCardEffect_RaisesMoveToSpaceEvent_WithCorrectTarget()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<MoveToSpaceCardEffect>();
            effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace;

            var channel = CreateChannel<MoveToSpaceEventChannel>();
            MoveToSpaceEvent captured = null;
            channel.Subscribe(e => captured = e);

            effect.moveToSpaceEventChannel = channel;

            effect.ApplyEffect(player);

            Assert.NotNull(captured, "MoveToSpaceEvent should be raised");
            Assert.AreEqual(player, captured.player);
            Assert.AreEqual(MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace, captured.targetKind);
        }

        // PayPlayerCardEffect  (simple “give money to player” card)

        [Test]
        public void PayPlayerCardEffect_RaisesPayPlayerEvent_WithCorrectAmount()
        {
            var player = CreatePlayer(0, 1000);

            var effect = CreateEffect<PayPlayerCardEffect>();
            effect.amount = 200;

            var channel = CreateChannel<PayPlayerEventChannel>();
            PayPlayerEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.payPlayerEventChannel = channel;

            effect.ApplyEffect(player);

            Assert.NotNull(captured, "PayPlayerEvent should be raised");
            Assert.AreEqual(player, captured.paidPlayer);
            Assert.AreEqual(200, captured.amountPaid);
        }

        // PayAllPlayersCardEffect / CollectFromAllPlayersCardEffect
        // These both use MoneyDistributionEvent with (Player, Amount).

        [Test]
        public void PayAllPlayersCardEffect_RaisesMoneyDistributionEvent_WithCorrectAmount()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<PayAllPlayersCardEffect>();
            effect.Amount = 50;

            var channel = CreateChannel<MoneyDistributionEventChannel>();
            MoneyDistributionEvent captured = null;
            channel.Subscribe(e => captured = e);

            effect.payAllPlayersEventChannel = channel;

            effect.ApplyEffect(player);

            Assert.NotNull(captured, "MoneyDistributionEvent should be raised");
            Assert.AreEqual(player, captured.Player);
            Assert.AreEqual(50, captured.Amount);
        }

        [Test]
        public void CollectFromAllPlayersCardEffect_RaisesMoneyDistributionEvent_WithCorrectAmount()
        {
            var player = CreatePlayer(1, 1500);

            var effect = CreateEffect<CollectFromAllPlayersCardEffect>();
            effect.Amount = 25;

            var channel = CreateChannel<MoneyDistributionEventChannel>();
            MoneyDistributionEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.collectFromAllPlayersEventChannel = channel;

            effect.ApplyEffect(player);

            Assert.NotNull(captured, "MoneyDistributionEvent should be raised");
            Assert.AreEqual(player, captured.Player);
            Assert.AreEqual(25, captured.Amount);
        }

        [Test]
        public void GoToJailCardEffect_RaisesEvent_InJail_WithConfiguredTurns()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<GoToJailCardEffect>();
            effect.TurnsInJail = 3;

            var channel = CreateChannel<JailStateChangedEventChannel>();
            JailStateChangedEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.JailStateChangedEventChannel = channel;

            effect.ApplyEffect(player);

            Assert.NotNull(captured, "JailStateChangedEvent should be raised");
            Assert.AreEqual(player, captured.player);
            Assert.IsTrue(captured.inJail);
            Assert.AreEqual(3, captured.jailTurns);
        }

        // CollectPerPropertyCardEffect / PayPerPropertyCardEffect
        //
        // These area bit  more complex (they look @ owned properties and upgrades).
        // For now we at least verify the early-exit behaviour: when the player
        // has NO owned properties, effects should NOT raise an event.
        // That still acts as the guard logic and verifies we don’t charge
        // or pay incorrectly.
        [Test]
        public void CollectPerPropertyCardEffect_NoOwnedProperties_DoesNotRaiseEvent()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<CollectPerPropertyCardEffect>();
            effect.ChargeForHouse = 40;
            effect.ChargeForHotel = 100;

            var channel = CreateChannel<PayPlayerEventChannel>();
            PayPlayerEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.payPlayerEventChannel = channel;

            // Player has no owned properties by default
            effect.ApplyEffect(player);

            Assert.IsNull(captured, "No PayPlayerEvent should be raised when player owns no properties");
        }



        [Test]
        public void PayPerPropertyCardEffect_NoOwnedProperties_DoesNotRaiseEvent()
        {
            var player = CreatePlayer(0, 1500);

            var effect = CreateEffect<PayPerPropertyCardEffect>();
            effect.ChargeForHouse = 40;
            effect.ChargeForHotel = 100;

            var channel = CreateChannel<ChargePlayerEventChannel>();
            ChargePlayerEvent captured = null;
            channel.Subscribe(e => captured = e);
            effect.chargePlayerEventChannel = channel;

            // Player has no owned properties by default
            effect.ApplyEffect(player);

            Assert.IsNull(captured, "No ChargePlayerEvent should be raised when player owns no properties");
        }
    }
}
