using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Strategy")]
    private IUpgradeStrategy strategy = new StandardUpgradeStrategy();

    public bool TryHandleUpgrade(Player owner, IUpgradableTileInfo tile, out UpgradeDecision decision)
    {
        decision = default;
        if (owner == null || tile == null) return false;

        decision = strategy.GetUpgradeDecision(tile, owner);
        if (!decision.Allowed) return false;

        if (!(tile is OwnableSpaceTileAdapter space)) return false;

        if (owner.TrySpend(decision.Cost))
        {
            space.ApplyUpgrade();
            return true;
        }
        return false;
    }
}