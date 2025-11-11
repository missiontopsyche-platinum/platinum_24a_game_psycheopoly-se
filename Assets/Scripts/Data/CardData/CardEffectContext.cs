public record CardEffectContext
{
    public Player player;
    public Player targetPlayer;

    public CardEffectContext(Player player, Player targetPlayer = null)
    {
        this.player = player;
        this.targetPlayer = targetPlayer; // Could be the banker or another player
    }
}
