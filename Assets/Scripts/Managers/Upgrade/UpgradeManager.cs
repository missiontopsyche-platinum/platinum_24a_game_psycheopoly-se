using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    
    // TODO: Add event channels
    private void Awake()
    {
        EnsureDependencies();
    }

    private void EnsureDependencies()
    {
        if (strategy == null)
        {
            strategy = new StandardUpgradeStrategy();
        }
        // TODO: Any channels to be subscribe to add it here.
    }

    public bool TryHandleUpgrade(Player owner, IUpgradableTileInfo tile, out UpgradeDecision decision)
    {
        decision = default;
        if (owner == null || tile == null) return false;

        decision = strategy.GetUpgradeDecision(tile, owner);
        if (!decision.Allowed) return false;

        if (!(tile is OwnableSpaceTileAdapter space)) return false;

        if (owner.TrySpend(decision.Cost) == Player.FinancialStatus.Success)
        {
            space.ApplyUpgrade();
            return true;
        }
        return false;
    }

    // Entry point
    // TODO: Finish implementation of this method, add event as parameter, and raise event after upgrade is successful.
    public void OnUpgradeRequest(/*TODO: pass in event data here*/)
    {
        /*if (Event == null) return;
        if (TryHandleUpgrade(TODO: pass in event data here))
        {
            if (decision is true)
             TODO: Raise event for successful upgrade here.
        }*/
    }
}