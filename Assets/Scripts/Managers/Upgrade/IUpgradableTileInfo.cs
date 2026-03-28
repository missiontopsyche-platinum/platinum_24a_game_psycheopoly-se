using Assets.Scripts.Managers.Rent;

public interface IUpgradableTileInfo
{
    string Name { get; }
    TileType Type { get; }
    ColorGroup Group { get; }
    bool IsMortgaged { get; }
    Player GetOwner();

    int UpgradeLevel { get; }
    int UpgradeCost { get; }
    int MaxUpgradeLevel { get; }
    bool IsMaxed { get; }
    int[] UpgradeCostByLevel { get; }

    int GetNextUpgradeCost();
    bool CanApplyUpgrade();
    void ApplyUpgrade();
}