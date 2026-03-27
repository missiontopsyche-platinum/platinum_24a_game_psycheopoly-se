public struct UpgradeResultEvent
{
    public bool Success;
    public UpgradeFailReason FailReason;
    public int UpgradeCost;
    public int NewUpgradeLevel;
    public int PlayerId;
    public int TileId;

    public UpgradeResultEvent(
        bool success,
        UpgradeFailReason failReason,
        int upgradeCost,
        int newUpgradeLevel,
        int playerId,
        int tileId)
    {
        Success = success;
        FailReason = failReason;
        UpgradeCost = upgradeCost;
        NewUpgradeLevel = newUpgradeLevel;
        PlayerId = playerId;
        TileId = tileId;
    }
}