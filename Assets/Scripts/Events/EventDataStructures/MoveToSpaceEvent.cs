
public class MoveToSpaceEvent
{
    public Player player;
    public MoveToSpaceCardEffect.TargetSpaceKind targetKind;

    public MoveToSpaceEvent(Player player, MoveToSpaceCardEffect.TargetSpaceKind targetKind)
    {
        this.player = player;
        this.targetKind = targetKind;
    }
}
