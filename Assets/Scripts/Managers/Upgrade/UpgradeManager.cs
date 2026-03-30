using Events.EventDataStructures;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Strategy")]
    private IUpgradeStrategy strategy = new StandardUpgradeStrategy();

    [Header("Event Channels")]
    [SerializeField] private UpgradeRequestEventChannel upgradeRequestEventChannel;
    // TODO: Add event channels
    private void Awake()
    {
        EnsureDependencies();
    }

    private void OnEnable()
    {
        upgradeRequestEventChannel?.Subscribe(OnUpgradeRequest);
    }

    private void OnDisable()
    {
        upgradeRequestEventChannel?.Unsubscribe(OnUpgradeRequest);
    }

    private void EnsureDependencies()
    {
        if (strategy == null)
        {
            strategy = new StandardUpgradeStrategy();
        }
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
    public void OnUpgradeRequest(UpgradeRequestEvent evt)
    {
        if (evt == null || evt.player == null || evt.property == null)
            return;

        // This only works if the property object itself implements IUpgradableTileInfo.
        // If your scene uses an adapter instead, this event payload will need to carry
        // that adapter or map from PropertySpaceData to OwnableSpaceTileAdapter.
        if (!(evt.property is IUpgradableTileInfo upgradableTile))
        {
            Logging.Logger.Warn("UpgradeManager.OnUpgradeRequest",
                "Upgrade request ignored because the property does not implement IUpgradableTileInfo.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        if (TryHandleUpgrade(evt.player, upgradableTile, out UpgradeDecision decision))
        {
            Logging.Logger.Info("UpgradeManager.OnUpgradeRequest",
                $"{evt.player.GetPName()} upgraded {evt.property.spaceName} for ${decision.Cost}.",
                Logging.LogCategory.Gameplay,
                this);
        }
        else
        {
            Logging.Logger.Debug("UpgradeManager.OnUpgradeRequest",
                $"{evt.player.GetPName()} could not upgrade {evt.property.spaceName}.",
                Logging.LogCategory.Gameplay,
                this);
        }
    }
}