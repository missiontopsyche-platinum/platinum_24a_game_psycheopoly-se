
public class MoveToSpaceEvent
{
    public Player player;
    public MoveToSpaceCardEffect.TargetMode targetMode;
    public MoveToSpaceCardEffect.TargetSpaceType targetKind;
    public MoveToSpaceCardEffect.SpecificBoardSpace specificBoardSpace;
    // supports None / DoubleRent / FreeRent
    public MoveToSpaceCardEffect.ArrivalRentModifierType arrivalRentModifier;

    public MoveToSpaceEvent(
        Player player,
        MoveToSpaceCardEffect.TargetMode targetMode,
        MoveToSpaceCardEffect.TargetSpaceType targetKind,
        MoveToSpaceCardEffect.SpecificBoardSpace specificBoardSpace,
        MoveToSpaceCardEffect.ArrivalRentModifierType arrivalRentModifier = MoveToSpaceCardEffect.ArrivalRentModifierType.None)
    {
        this.player = player;
        this.targetMode = targetMode;
        this.targetKind = targetKind;
        this.specificBoardSpace = specificBoardSpace;
        this.arrivalRentModifier = arrivalRentModifier;
    }
}
