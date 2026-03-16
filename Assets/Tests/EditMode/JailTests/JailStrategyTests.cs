using System;
using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Jail;
using System.Reflection;
using Assets.Scripts.Managers.Rules;

/// <summary>
/// this contains all tests for the JailStrategy as it currently operates
/// if any changes are made to functionality in the future, tests may need updates
/// </summary>
namespace Assets.Tests.EditMode.JailTests
{
    public class JailStrategyTests
    {

        private Player testPlayer;
        private JailUtility jailUtility;
        private IRuleSet RuleSet;

        [SetUp]
        public void SetUp()
        {
            testPlayer = ScriptableObject.CreateInstance<Player>();
            testPlayer.SetPName("TestPlayer");
            testPlayer.SetMoney(500);
            testPlayer.SetInJail(true);
            testPlayer.SetJailTurns(0);

            RuleSet = new StandardRuleSet();

            jailUtility = new JailUtility(RuleSet);
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
            jailUtility.AttemptEscape(testPlayer, 3, 3);

            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after rolling doubles.");
            Assert.AreEqual(0, testPlayer.GetJailTurns(), "Jail turns should reset after release.");
        }

        //the tests that a use is in jail, doesn't roll doubles, and remains in jail.
        [Test]
        public void AttemptEscape_NoDoubles_StillInJail()
        {
            jailUtility.AttemptEscape(testPlayer, 2, 5);

            Assert.IsTrue(testPlayer.GetInJail(), "Player should still be in jail after failing to roll doubles.");
            Assert.AreEqual(1, testPlayer.GetJailTurns(), "Jail turns should increment by one.");
        }

        //this tests the forced exit which forces a fee-pay
        [Test]
        public void ForcedExit_AfterThreeTurns_PaysFeeAndReleasesPlayer()
        {
            testPlayer.SetJailTurns(3);
            testPlayer.SetMoney(200);

            jailUtility.ForcedExit(testPlayer);

            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after forced exit.");
            Assert.AreEqual(100, testPlayer.GetMoney(), "Player should pay $100 to exit jail.");
        }

        //this tests pulling money in the force-pay and that the players funds are updated
        [Test]
        public void PayFee_EnoughMoney_ReleasesPlayerAndDeductsFee()
        {
            jailUtility.PayFee(testPlayer);

            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after paying fee.");
            Assert.AreEqual(400, testPlayer.GetMoney(), "Money should decrease by $100 after paying fee.");
        }

        //this is our current fucntionality. when we implement bankruptcy rules, it'll change...
        [Test]
        public void PayFee_NotEnoughMoney_PlayerStaysInJail()
        {
            testPlayer.SetMoney(50);

            jailUtility.PayFee(testPlayer);

            Assert.IsTrue(testPlayer.GetInJail(), "Player should remain in jail if they cannot pay the fee.");
            Assert.AreEqual(50, testPlayer.GetMoney(), "Player’s money should not change if payment fails.");
        }

        [Test]
        public void UseGetOutOfJailFree_WithChanceCard_ReleasesPlayer()
        {
            // Using reflection because the fields are private
            typeof(Player).GetField("getOutOfJailFree_Chance", BindingFlags.NonPublic | BindingFlags.Instance)
                          .SetValue(testPlayer, 1);

            jailUtility.UseGetOutOfJailFree(testPlayer);

            int remainingChanceCards = (int)typeof(Player).GetField("getOutOfJailFree_Chance", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          .GetValue(testPlayer);

            Assert.AreEqual(0, remainingChanceCards, "Chance card should be decremented after use.");
            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after using Get Out of Jail Free card.");
        }

        [Test]
        public void UseGetOutOfJailFree_WithCommunityCard_ReleasesPlayer()
        {
            typeof(Player).GetField("getOutOfJailFree_Community", BindingFlags.NonPublic | BindingFlags.Instance)
                          .SetValue(testPlayer, 1);

            jailUtility.UseGetOutOfJailFree(testPlayer);

            int remainingCommunityCards = (int)typeof(Player).GetField("getOutOfJailFree_Community", BindingFlags.NonPublic | BindingFlags.Instance)
                                                             .GetValue(testPlayer);

            Assert.AreEqual(0, remainingCommunityCards, "Community card should be decremented after use.");
            Assert.IsFalse(testPlayer.GetInJail(), "Player should be released after using Get Out of Jail Free card.");
        }

        [Test]
        public void UseGetOutOfJailFree_NoCards_PlayerStaysInJail()
        {
            jailUtility.UseGetOutOfJailFree(testPlayer);

            Assert.IsTrue(testPlayer.GetInJail(), "Player should remain in jail if they have no cards.");
        }
    }
}
