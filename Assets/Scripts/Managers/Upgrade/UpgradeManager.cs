using UnityEngine;
using Events.EventDataStructures;

public class UpgradeManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private UpgradeRequestEventChannel upgradeRequestChannel;
    [SerializeField] private UpgradeResultEventChannel upgradeResultChannel;

    private void OnEnable()
    {
        if (upgradeRequestChannel != null)
        {
            upgradeRequestChannel.Subscribe(OnUpgradeRequest);
        }
    }

    private void OnDisable()
    {
        if (upgradeRequestChannel != null)
        {
            upgradeRequestChannel.Unsubscribe(OnUpgradeRequest);
        }
    }

    public bool TryHandleUpgrade(Player owner, PropertySpaceData tile, out UpgradeDecision decision)
    {
        decision = default;

        if (owner == null || tile == null)
            return false;

        var validProperties = owner.GetValidUpgradableProperties();
        var matchingGroup = new System.Collections.Generic.List<PropertySpaceData>();

        foreach (var property in validProperties)
        {
            if (property != null && property.Group == tile.Group)
            {
                matchingGroup.Add(property);
            }
        }

        PropertySpaceData[] monopolyGroup = matchingGroup.ToArray();

        decision = UpgradeUtility.Evaluate(owner, tile, monopolyGroup);

        if (!decision.Allowed)
            return false;

        return UpgradeUtility.TryExecute(owner, tile, decision);
    }

    // Entry point
    private void OnUpgradeRequest(UpgradeRequestEvent request)
    {
        if (request.Player == null || request.Tile == null)
        {
            RaiseResult(
                false,
                UpgradeDecision.Failed(UpgradeFailReason.InvalidRequest),
                request.Player,
                request.Tile);
            return;
        }

        bool success = TryHandleUpgrade(request.Player, request.Tile, out UpgradeDecision decision);
        RaiseResult(success, decision, request.Player, request.Tile);
    }

    private void RaiseResult(bool success, UpgradeDecision decision, Player player, PropertySpaceData tile)
    {
        var result = new UpgradeResultEvent(
            success: success,
            failReason: success ? UpgradeFailReason.None : decision.FailReason,
            upgradeCost: decision.Cost,
            newUpgradeLevel: tile != null ? tile.GetCurrentUpgradeLevel() : 0,
            player: player,
            tile: tile
        );

        upgradeResultChannel?.RaiseEvent(result);
    }
   
}
