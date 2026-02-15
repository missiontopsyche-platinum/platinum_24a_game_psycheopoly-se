using UnityEngine;

public interface IUpgradeStrategy
{
    UpgradeDecision GetUpgradeDecision(IUpgradableTileInfo tile, Player owner);
}

public struct UpgradeDecision
{
    public bool Allowed;
    public int Cost;
}