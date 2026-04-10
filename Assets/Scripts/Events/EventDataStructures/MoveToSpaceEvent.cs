
public class MoveToSpaceEvent
{
    public Player player;
    public MoveToSpaceCardEffect.TargetMode targetMode;
    public MoveToSpaceCardEffect.TargetSpaceType targetKind;
    public MoveToSpaceCardEffect.SpecificBoardSpace specificBoardSpace;

    public MoveToSpaceEvent(
        Player player,
        MoveToSpaceCardEffect.TargetMode targetMode,
        MoveToSpaceCardEffect.TargetSpaceType targetKind,
        MoveToSpaceCardEffect.SpecificBoardSpace specificBoardSpace)
    {
        this.player = player;
        this.targetMode = targetMode;
        this.targetKind = targetKind;
        this.specificBoardSpace = specificBoardSpace;
    }
}
