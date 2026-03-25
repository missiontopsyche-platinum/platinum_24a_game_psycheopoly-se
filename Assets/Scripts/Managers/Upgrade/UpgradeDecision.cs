public enum UpgradeFailReason
{
    None = 0,
    InvalidRequest,
    NotStreet,
    Mortgaged,
    AlreadyMaxed,
    InvalidUpgradeCost,
    InsufficientFunds,
    NotOwner,
    MonopolyNotOwned,
    UnevenBuilding,
    ExecutionFailed
}

public struct UpgradeDecision
{
    public bool Allowed;
    public int Cost;
    public UpgradeFailReason FailReason;

    public static UpgradeDecision Success(int cost)
    {
        return new UpgradeDecision
        {
            Allowed = true,
            Cost = cost,
            FailReason = UpgradeFailReason.None
        };
    }

    public static UpgradeDecision Failed(UpgradeFailReason failReason, int cost = 0)
    {
        return new UpgradeDecision
        {
            Allowed = false,
            Cost = cost,
            FailReason = failReason
        };
    }
}