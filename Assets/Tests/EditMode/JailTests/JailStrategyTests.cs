using System;
using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Jail;
using System.Reflection;

namespace Tests.EditMode.JailTests
{
    public class JailStrategyTests
    {
        private Player testPlayer;

        [SetUp]
        public void SetUp()
        {
            testPlayer = ScriptableObject.CreateInstance<Player>();
            testPlayer.SetPName("TestPlayer");
            testPlayer.SetMoney(500);
            testPlayer.SetInJail(true);
            testPlayer.SetJailTurns(0);

            typeof(Player).GetField("getOutOfJailFree_Chance", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(testPlayer, 0);

            typeof(Player).GetField("getOutOfJailFree_Community", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(testPlayer, 0);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(testPlayer);
        }

        //this tests the doubles-roll escape
        [Test]
        public void AttemptEscape_RollsDoubles_ReleasesPlayer()
        {
            var result = JailUtility.AttemptEscape(testPlayer, 3, 3);

            Assert.AreEqual(result, JailUtility.EscapeAttemptResult.Escaped);
            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after rolling doubles.");
            Assert.AreEqual(0, testPlayer.GetJailTurns(), "Jail turns should reset after release.");
        }

        //the tests that a use is in jail, doesn't roll doubles, and remains in jail.
        [Test]
        public void AttemptEscape_NoDoubles_StillInJail()
        {
            var result = JailUtility.AttemptEscape(testPlayer, 2, 5);

            Assert.AreEqual(result, JailUtility.EscapeAttemptResult.Failed);
            Assert.IsTrue(testPlayer.GetInJail(), "Player should still be in jail after failing to roll doubles.");
            Assert.AreEqual(1, testPlayer.GetJailTurns(), "Jail turns should increment by one.");
        }

        //this tests the forced exit which forces a fee-pay
        [Test]
        public void ForcedExit_AfterThreeTurns_PaysFeeAndReleasesPlayer()
        {
            testPlayer.SetJailTurns(3);
            testPlayer.SetMoney(200);

            var result = JailUtility.AttemptEscape(testPlayer, 3, 1);

            Assert.AreEqual(result, JailUtility.EscapeAttemptResult.ForcedExitPaid);
            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after forced exit.");
            Assert.AreEqual(100, testPlayer.GetMoney(), "Player should pay $100 to exit jail.");
        }

        //this tests pulling money in the force-pay and that the players funds are updated
        [Test]
        public void PayFee_EnoughMoney_ReleasesPlayerAndDeductsFee()
        {
            var result = JailUtility.PayFee(testPlayer);

            Assert.AreEqual(result, JailUtility.FeePaymentResult.Paid);
            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after paying fee.");
            Assert.AreEqual(400, testPlayer.GetMoney(), "Money should decrease by $100 after paying fee.");
        }
        
        [Test]
        public void PayFee_NotEnoughMoney_PlayerStaysInJail()
        {
                testPlayer.SetMoney(50);

            var result = JailUtility.PayFee(testPlayer);

            Assert.AreEqual(JailUtility.FeePaymentResult.Bankrupt, result);
            Assert.IsTrue(testPlayer.GetInJail());
            Assert.AreEqual(50, testPlayer.GetMoney());
        }

        [Test]
        public void UseGetOutOfJailFree_ChanceFieldOnly_NoCardAvailable()
        {
            var result = JailUtility.UseGetOutOfJailFree(testPlayer);

            Assert.AreEqual(JailUtility.CardUseResult.NoCardAvailable, result);
            Assert.IsTrue(testPlayer.GetInJail());
        }

        [Test]
        public void UseGetOutOfJailFree_CommunityFieldOnly_NoCardAvailable()
        {
            var result = JailUtility.UseGetOutOfJailFree(testPlayer);

            Assert.AreEqual(JailUtility.CardUseResult.NoCardAvailable, result);
            Assert.IsTrue(testPlayer.GetInJail());
        }

        [Test]
        public void UseGetOutOfJailFree_NoCards_PlayerStaysInJail()
        {
            var result = JailUtility.UseGetOutOfJailFree(testPlayer);

            Assert.AreEqual(result, JailUtility.CardUseResult.NoCardAvailable);
            Assert.IsTrue(testPlayer.GetInJail(), "Player should remain in jail if they have no cards.");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            Assert.NotNull(field, $"Missing field: {fieldName}");
            field.SetValue(target, value);
        }
    }
}
