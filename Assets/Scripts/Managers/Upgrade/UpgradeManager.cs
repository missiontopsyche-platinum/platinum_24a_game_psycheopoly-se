using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private UpgradeRequestEventChannel upgradeRequestChannel;
    [SerializeField] private UpgradeResultEventChannel upgradeResultChannel;

    private void OnEnable()
    {
        if (upgradeRequestChannel != null)
        {
            upgradeRequestChannel.OnEventRaised += OnUpgradeRequest;
        }
    }

    private void OnDisable()
    {
        if (upgradeRequestChannel != null)
        {
            upgradeRequestChannel.OnEventRaised -= OnUpgradeRequest;
        }
    }

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
   private void OnUpgradeRequest(UpgradeRequestEvent request)
    {
        Player owner = ResolvePlayer(request.PlayerId);
        IUpgradableTileInfo tile = ResolveTile(request.TileId);

        UpgradeDecision decision = UpgradeUtility.Evaluate(owner, tile);

        if (!decision.Allowed)
        {
            RaiseResult(false, decision, request, tile);
            return;
        }

        bool success = UpgradeUtility.TryExecute(owner, tile, decision);

        RaiseResult(success, decision, request, tile);
    }

    private void RaiseResult(bool success, UpgradeDecision decision, UpgradeRequestEvent request, IUpgradableTileInfo tile)
    {
        var result = new UpgradeResultEvent(
            success: success,
            failReason: success ? UpgradeFailReason.None : decision.FailReason,
            upgradeCost: decision.Cost,
            newUpgradeLevel: tile != null ? tile.UpgradeLevel : 0,
            playerId: request.PlayerId,
            tileId: request.TileId
        );

        upgradeResultChannel?.RaiseEvent(result);
    }

      private Player ResolvePlayer(int playerId)
    {
        //TODO hook into player manager
        return null;
    }

    private IUpgradableTileInfo ResolveTile(int tileId)
    {
        //TODO hook into tile 
        return null;
    }
}