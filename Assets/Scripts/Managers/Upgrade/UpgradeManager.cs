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
        // TODO: Any channels to be subscribe to add it here.
    }

    public bool TryHandleUpgrade(Player owner, IUpgradableTileInfo tile, out UpgradeDecision decision)
    {
        decision = default;

        if (owner == null || tile == null)
            return false;

        decision = UpgradeUtility.Evaluate(owner, tile);
        if (!decision.Allowed)
            return false;

        return UpgradeUtility.TryExecute(owner, tile, decision);
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