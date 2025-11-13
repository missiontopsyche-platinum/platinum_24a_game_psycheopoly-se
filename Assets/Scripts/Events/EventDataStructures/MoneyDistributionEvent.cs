
public class MoneyDistributionEvent
{
    public int Amount;
    public Player Player;

    public MoneyDistributionEvent(Player player, int amount)
    {
        Player = player;
        Amount = amount;
    }
}
