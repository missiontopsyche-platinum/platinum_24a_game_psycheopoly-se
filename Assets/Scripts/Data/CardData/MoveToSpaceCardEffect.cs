using UnityEngine;

/// <summary>
/// Requests that a player be moved to a specific type of board space,
/// such as GO, the nearest property, or the nearest card-drawing space.
/// </summary>
[CreateAssetMenu(fileName = "MoveToSpaceCardEffect", menuName = "Card Data/Effects/MoveToSpaceCardEffect")]
public class MoveToSpaceCardEffect : CardEffect
{
    // TODO: Not sure what spaces we need atm, will be expanded on Sprint 4
    public enum TargetMode
    {
        NearestByType,
        SpecificSpace
    }

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

    //exact card destinations
    public enum SpecificBoardSpace
    {
        None = -1,
        Go = 0,
        MultispectralImager = 5,
        Themis = 11,
        Eunomia = 24,
        Ceres = 39
    }

    public TargetMode targetMode = TargetMode.NearestByType;
    public TargetSpaceType targetType;
    public SpecificBoardSpace specificBoardSpace = SpecificBoardSpace.None;

    public MoveToSpaceEventChannel moveToSpaceEventChannel;
    

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player) || moveToSpaceEventChannel == null)
            return;

        var evt = new MoveToSpaceEvent(player, targetMode, targetType, specificBoardSpace);
        moveToSpaceEventChannel.RaiseEvent(evt);
    }
}
