using PsycheOpoly.Board;

public class MoveToSpaceCardEffect : CardEffect
{
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
