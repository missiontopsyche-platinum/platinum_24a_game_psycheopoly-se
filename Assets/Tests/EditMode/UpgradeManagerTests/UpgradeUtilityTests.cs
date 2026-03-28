using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.Managers.Rent;

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

        private class FakeUpgradableTile : IUpgradableTileInfo
        {
            public string Name { get; set; } = "Test Property";
            public TileType Type { get; set; } = TileType.Street;
            public ColorGroup Group { get; set; } = ColorGroup.Brown;
            public bool IsMortgaged { get; set; } = false;
            public Player Owner { get; set; }

            public int UpgradeLevel { get; set; } = 0;
            public int UpgradeCost { get; set; } = 50;
            public int MaxUpgradeLevel { get; set; } = 2;
            public bool IsMaxed => UpgradeLevel >= MaxUpgradeLevel;
            public int[] UpgradeCostByLevel { get; set; } = new[] { 50, 50, 50 };

            public bool ApplyUpgradeCalled { get; private set; }

            public Player GetOwner() => Owner;
            public int GetNextUpgradeCost() => UpgradeCost;
            public bool CanApplyUpgrade() => !IsMaxed && UpgradeCost > 0;

            public void ApplyUpgrade()
            {
                ApplyUpgradeCalled = true;
                UpgradeLevel++;
            }
        }

        [Test]
        public void EvaluateFail_WhenOwnerIsNull()
        {
            var tile = new FakeUpgradableTile();

            var result = UpgradeUtility.Evaluate(null, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InvalidRequest, result.FailReason);
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

            var tile = new FakeUpgradableTile
            {
                Owner = actualOwner,
                UpgradeCost = 50
            };

            var result = UpgradeUtility.Evaluate(requester, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.NotOwner, result.FailReason);

            Object.DestroyImmediate(requester);
            Object.DestroyImmediate(actualOwner);
        }

        [Test]
        public void EvaluateFail_WhenTileIsNotStreet()
        {
            var owner = CreatePlayer(500);

            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                Type = (TileType)(-1)
            };

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.NotStreet, result.FailReason);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void EvaluateFail_WhenMortgaged()
        {
            var owner = CreatePlayer(500);

            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                IsMortgaged = true
            };

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.Mortgaged, result.FailReason);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void EvaluateFail_WhenAlreadyMaxed()
        {
            var owner = CreatePlayer(500);

            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                UpgradeLevel = 2,
                MaxUpgradeLevel = 2
            };

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.AlreadyMaxed, result.FailReason);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void EvaluateFail_WhenUpgradeCostInvalid()
        {
            var owner = CreatePlayer(500);

            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 0
            };

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InvalidUpgradeCost, result.FailReason);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void EvaluateFail_WhenInsufficientFunds()
        {
            var owner = CreatePlayer(10);

            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50
            };

            var result = UpgradeUtility.Evaluate(owner, tile, new[] { tile });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.InsufficientFunds, result.FailReason);
            Assert.AreEqual(50, result.Cost);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void EvaluateFail_WhenFullMonopolyNotOwned()
        {
            var owner = CreatePlayer(500);
            var otherOwner = CreatePlayer(500);

            var tile1 = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                Group = ColorGroup.Brown
            };

            var tile2 = new FakeUpgradableTile
            {
                Owner = otherOwner,
                UpgradeCost = 50,
                Group = ColorGroup.Brown
            };

            var result = UpgradeUtility.Evaluate(owner, tile1, new[] { tile1, tile2 });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.MonopolyNotOwned, result.FailReason);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(otherOwner);
        }

        [Test]
        public void EvaluateFail_WhenBuildingUnevenly()
        {
            var owner = CreatePlayer(500);

            var tile1 = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                Group = ColorGroup.Brown,
                UpgradeLevel = 1
            };

            var tile2 = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                Group = ColorGroup.Brown,
                UpgradeLevel = 0
            };

            var result = UpgradeUtility.Evaluate(owner, tile1, new[] { tile1, tile2 });

            Assert.IsFalse(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.UnevenBuilding, result.FailReason);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void EvaluateSuccess_WhenAllRulesPass()
        {
            var owner = CreatePlayer(500);

            var tile1 = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                Group = ColorGroup.Brown,
                UpgradeLevel = 0
            };

            var tile2 = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                Group = ColorGroup.Brown,
                UpgradeLevel = 0
            };

            var result = UpgradeUtility.Evaluate(owner, tile1, new[] { tile1, tile2 });

            Assert.IsTrue(result.Allowed);
            Assert.AreEqual(UpgradeFailReason.None, result.FailReason);
            Assert.AreEqual(50, result.Cost);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void TryExecuteFalse_WhenDecisionNotAllowed()
        {
            var owner = CreatePlayer(500);
            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50
            };

            var decision = UpgradeDecision.Failed(UpgradeFailReason.InvalidRequest);

            var result = UpgradeUtility.TryExecute(owner, tile, decision);

            Assert.IsFalse(result);
            Assert.IsFalse(tile.ApplyUpgradeCalled);

            Object.DestroyImmediate(owner);
        }

        [Test]
        public void TryExecuteTrue_AppliesUpgradeWhenAllowed()
        {
            var owner = CreatePlayer(500);
            var tile = new FakeUpgradableTile
            {
                Owner = owner,
                UpgradeCost = 50,
                UpgradeLevel = 0
            };

            var decision = UpgradeDecision.Success(50);

            var result = UpgradeUtility.TryExecute(owner, tile, decision);

            Assert.IsTrue(result);
            Assert.IsTrue(tile.ApplyUpgradeCalled);
            Assert.AreEqual(1, tile.UpgradeLevel);

            Object.DestroyImmediate(owner);
        }
    }
}