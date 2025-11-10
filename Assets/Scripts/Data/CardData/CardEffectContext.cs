using System.Collections.Generic;

public record CardEffectContext
{
    public Player player;
    public Player targetPlayer;
    public ICardEventPublisher EventPublisher;

    public CardEffectContext(Player player, ICardEventPublisher eventPublisher, Player targetPlayer = null)
    {
        this.player = player;
        this.EventPublisher = eventPublisher;
        this.targetPlayer = targetPlayer;
    }
}
