using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.UpgradeManagerTests
{
    public class UpgradeUtilityTests
    {
        private Player CreatePlayer(int money)
        {
            var player = ScriptableObject.CreateInstance<Player>();
            player.SetMoney(money);
            return player;
        }

        private PropertySpaceData CreateProperty(
            Player owner,
            int moneyCost = 50,
            int upgradeLevel = 0,
            bool mortgaged = false)
        {
            var pd = ScriptableObject.CreateInstance<PropertySpaceData>();
            pd.SetResearchFundingValues(new[] { 10, 20, 30 });
            pd.SetUpgradeCostByLevel(new[] { 50, 50, 50 });
            pd.SetDataPointCost(moneyCost);
            pd.SetUpgradeLevel(upgradeLevel);
            pd.SetOwner(owner);
            pd.isMortgaged = mortgaged;
            return pd;
        }

        [Test]
        public void EvaluateFail_WhenOwnerIsNull()
        {
            var tile = CreateProperty(null);

            var result = UpgradeUtility.Evaluate(null, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InvalidRequest, result.FailReason);

            Object.DestroyImmediate(tile);
        }

        [Test]
        public void EvaluateFail_WhenTileIsNull()
        {
            var player = CreatePlayer(500);

            var result = UpgradeUtility.Evaluate(player, null, null);

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InvalidRequest, result.FailReason);

            Object.DestroyImmediate(player);
        }

        [Test]
        public void EvaluateFail_WhenPlayerDoesNotOwnTile()
        {
            var requester = CreatePlayer(500);
            var actualOwner = CreatePlayer(500);
            var tile = CreateProperty(actualOwner);

            var result = UpgradeUtility.Evaluate(requester, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.NotOwner, result.FailReason);

            Object.DestroyImmediate(requester);
            Object.DestroyImmediate(actualOwner);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void EvaluateFail_WhenMortgaged()
        {
            var owner = CreatePlayer(500);
            var tile = CreateProperty(owner, mortgaged: true);

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.Mortgaged, result.FailReason);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void EvaluateFail_WhenAlreadyMaxed()
        {
            var owner = CreatePlayer(500);
            var tile = CreateProperty(owner, upgradeLevel: 2);

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.AlreadyMaxed, result.FailReason);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void EvaluateFail_WhenUpgradeCostInvalid()
        {
            var owner = CreatePlayer(500);
            var tile = CreateProperty(owner, moneyCost: 0);

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InvalidUpgradeCost, result.FailReason);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void EvaluateFail_WhenInsufficientFunds()
        {
            var owner = CreatePlayer(10);
            var tile = CreateProperty(owner, moneyCost: 50);

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InsufficientFunds, result.FailReason);
            Assert.AreEqual(50, result.Cost);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void EvaluateFail_WhenFullMonopolyNotOwned()
        {
            var owner = CreatePlayer(500);
            var otherOwner = CreatePlayer(500);

            var tile1 = CreateProperty(owner);
            var tile2 = CreateProperty(otherOwner);

            var result = UpgradeUtility.Evaluate(owner, tile1, new[] { tile1, tile2 });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.MonopolyNotOwned, result.FailReason);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(otherOwner);
            Object.DestroyImmediate(tile1);
            Object.DestroyImmediate(tile2);
        }

        [Test]
        public void EvaluateFail_WhenBuildingUnevenly()
        {
            var owner = CreatePlayer(500);

            var tile1 = CreateProperty(owner, upgradeLevel: 1);
            var tile2 = CreateProperty(owner, upgradeLevel: 0);

            var result = UpgradeUtility.Evaluate(owner, tile1, new[] { tile1, tile2 });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.UnevenBuilding, result.FailReason);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile1);
            Object.DestroyImmediate(tile2);
        }

        [Test]
        public void EvaluateSuccess_WhenAllRulesPass()
        {
            var owner = CreatePlayer(500);

            var tile1 = CreateProperty(owner, upgradeLevel: 0);
            var tile2 = CreateProperty(owner, upgradeLevel: 0);

            var result = UpgradeUtility.Evaluate(owner, tile1, new[] { tile1, tile2 });

            Assert.IsTrue(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.None, result.FailReason);
            Assert.AreEqual(tile1.GetNextUpgradeCost(), result.Cost);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile1);
            Object.DestroyImmediate(tile2);
        }

        [Test]
        public void TryExecuteFalse_WhenDecisionNotAllowed()
        {
            var owner = CreatePlayer(500);
            var tile = CreateProperty(owner);

            var decision = UpgradeDecision.Failed(UpgradeFailReason.InvalidRequest);

            var result = UpgradeUtility.TryExecute(owner, tile, decision);

            Assert.IsFalse(result);
            Assert.AreEqual(0, tile.GetCurrentUpgradeLevel());

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile);
        }

        [Test]
        public void TryExecuteTrue_AppliesUpgradeWhenAllowed()
        {
            var owner = CreatePlayer(500);
            var tile = CreateProperty(owner, upgradeLevel: 0);

            var decision = UpgradeDecision.Success(tile.GetNextUpgradeCost());

            var result = UpgradeUtility.TryExecute(owner, tile, decision);

            Assert.IsTrue(result);
            Assert.AreEqual(1, tile.GetCurrentUpgradeLevel());

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(tile);
        }
    }
}