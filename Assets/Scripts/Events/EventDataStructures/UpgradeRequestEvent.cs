public struct UpgradeRequestEvent
{
    public int PlayerId;
    public int TileId;

    public UpgradeRequestEvent(int playerId, int tileId)
    {
        PlayerId = playerId;
        TileId = tileId;
    }
}