public struct UpgradeResultEvent
{
    public bool Success;
    public UpgradeFailReason FailReason;
    public int UpgradeCost;
    public int NewUpgradeLevel;
    public Player Player;
    public PropertySpaceData Tile;

    public UpgradeResultEvent(
        bool success,
        UpgradeFailReason failReason,
        int upgradeCost,
        int newUpgradeLevel,
        Player player,
        PropertySpaceData tile)
    {
        Success = success;
        FailReason = failReason;
        UpgradeCost = upgradeCost;
        NewUpgradeLevel = newUpgradeLevel;
        Player = player;
        Tile = tile;
    }
}