
public class MoneyDistributionEvent
{
    public enum MoneyDistributionEventType
    {
        Collect,
        Pay
    }

    public readonly MoneyDistributionEventType Type;
    public readonly int Amount;
    public readonly Player Player;

    public MoneyDistributionEvent(MoneyDistributionEventType type, Player player, int amount)
    {
        Type = type;
        Player = player;
        Amount = amount;
    }
}
