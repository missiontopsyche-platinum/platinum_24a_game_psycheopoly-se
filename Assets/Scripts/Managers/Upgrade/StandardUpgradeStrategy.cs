using Assets.Scripts.Managers.Rent;
using UnityEngine;

public class StandardUpgradeStrategy : IUpgradeStrategy
{
    public UpgradeDecision GetUpgradeDecision(IUpgradableTileInfo tile, Player owner)
    {
        if (tile == null || owner == null)
            return new UpgradeDecision { Allowed = false, Cost = 0 };

        if (tile.Type != TileType.Street)
            return new UpgradeDecision { Allowed = false, Cost = 0 };

        if (tile.IsMortgaged)
            return new UpgradeDecision { Allowed = false, Cost = 0 };

        if (tile.IsMaxed)
            return new UpgradeDecision { Allowed = false, Cost = 0 };

        int cost = Mathf.Max(0, tile.GetNextUpgradeCost());
        if (cost <= 0)
            return new UpgradeDecision { Allowed = false, Cost = 0 };

        if (owner.GetMoney() < cost)
            return new UpgradeDecision { Allowed = false, Cost = cost };

        return new UpgradeDecision { Allowed = true, Cost = cost };
    }
}
