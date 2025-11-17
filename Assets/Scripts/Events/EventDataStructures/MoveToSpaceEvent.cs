
public class MoveToSpaceEvent
{
    public Player player;
    public MoveToSpaceCardEffect.TargetSpaceType targetKind;

    public MoveToSpaceEvent(Player player, MoveToSpaceCardEffect.TargetSpaceType targetKind)
    {
        this.player = player;
        this.targetKind = targetKind;
    }
}
