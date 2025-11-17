using UnityEngine;

/// <summary>
/// Requests that a player be moved to a specific type of board space,
/// such as GO, the nearest property, or the nearest card-drawing space.
/// </summary>
[CreateAssetMenu(fileName = "MoveToSpaceCardEffect", menuName = "Card Data/Effects/MoveToSpaceCardEffect")]
public class MoveToSpaceCardEffect : CardEffect
{
    // TODO: Not sure what spaces we need atm, will be expanded on Sprint 4
    // to take into account all space data.
    public enum TargetSpaceKind
    {
        Go,
        NearestProperty,
        NearestCardSpace
    }

    public TargetSpaceKind targetKind;
    public MoveToSpaceEventChannel moveToSpaceEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (player == null || moveToSpaceEventChannel == null)
            return;

        var evt = new MoveToSpaceEvent(player, targetKind);
        moveToSpaceEventChannel.RaiseEvent(evt);
    }
}
