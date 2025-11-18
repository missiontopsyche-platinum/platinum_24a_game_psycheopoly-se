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
    public enum TargetSpaceType
    {
        CardSpace,
        ChargeSpace,
        GoForLaunchSpace,
        GoSpace,
        GravityAssistSpace,
        InstrumentSpace,
        LaunchPadSpace,
        PlanetSpace,
        PropertySpace
    }

    public TargetSpaceType targetType;
    public MoveToSpaceEventChannel moveToSpaceEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (player == null || moveToSpaceEventChannel == null)
            return;

        var evt = new MoveToSpaceEvent(player, targetType);
        moveToSpaceEventChannel.RaiseEvent(evt);
    }
}
