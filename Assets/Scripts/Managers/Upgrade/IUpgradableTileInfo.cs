using Assets.Scripts.Managers.Rent;
using UnityEngine;

public interface IUpgradableTileInfo : ITileRentInfo
{
    int UpgradeLevel { get; }
    int UpgradeCost { get; }
    int MaxUpgradeLevel { get; }
    bool IsMaxed { get; }
    int[] UpgradeCostByLevel { get; }
    int GetNextUpgradeCost();
}
