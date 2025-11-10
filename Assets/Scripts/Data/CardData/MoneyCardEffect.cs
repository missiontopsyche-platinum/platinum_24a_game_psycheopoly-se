using Events.EventDataStructures;
using UnityEngine;

public class MoneyCardEffect : CardEffect
{
    public enum EffectType
    {
        CollectFromBank,
        PayToBank,
        CollectPerProperty,
        PayPerProperty,
        CollectFromAll,
        PayToAll,
        // The bottom might not be necessary, but just in case
        CollectFromPlayer,
        PayToPlayer
    };
    [SerializeField] public EffectType effectType;
    [SerializeField] public int amount;

    public override void ApplyEffect(CardEffectContext context)
    {
        if (context == null)
        {
            Logging.Logger.Warn("MoneyCardEffect.ApplyEffect",
                "CardEffectContext is null.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        Player player = context.player;
        Player targetPlayer = context.targetPlayer;
        ICardEventPublisher eventPublisher = context?.EventPublisher;

        if (player == null || eventPublisher == null)
        {
            Logging.Logger.Warn("MoneyCardEffect.ApplyEffect",
                "Player or EventPublisher is null in CardEffectContext.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        // TODO: We need a centralized banking system to handle money transactions
        switch (effectType)
        {
            case EffectType.CollectFromBank:
                collectFromBank(player, targetPlayer, eventPublisher);
                break;
            case EffectType.PayToBank:
                payToBank(player, targetPlayer, eventPublisher);
                break;
            case EffectType.CollectPerProperty:
                collectPerProperty(player, targetPlayer, eventPublisher);
                break;
            case EffectType.PayPerProperty:
                payPerProperty(player, targetPlayer, eventPublisher);
                break;
            case EffectType.CollectFromAll:
                // TODO: Implement
                break;
            case EffectType.PayToAll:
                // TODO: Implement
                break;
            case EffectType.CollectFromPlayer:
                collectFromPlayer(player, targetPlayer, eventPublisher);
                break;
            case EffectType.PayToPlayer:
                payToPlayer(player, targetPlayer, eventPublisher);
                break;
            default:
                Logging.Logger.Warn("MoneyCardEffect.ApplyEffect",
                    $"Unknown EffectType: {effectType}",
                    Logging.LogCategory.Gameplay,
                    this);
                break;
        }
    }

    private void collectFromBank(Player player, Player banker, ICardEventPublisher eventPublisher)
    {
        eventPublisher.Publish(new ChargePlayerEvent(banker, amount)); // TODO: This should charge the bank or banker player
        eventPublisher.Publish(new PayPlayerEvent(player, amount));
    }

    private void payToBank(Player player, Player banker, ICardEventPublisher eventPublisher)
    {
        eventPublisher.Publish(new ChargePlayerEvent(player, amount));
        eventPublisher.Publish(new PayPlayerEvent(banker, amount)); // TODO: The bank or the banker player should receive this
    }

    private void collectPerProperty(Player player, Player banker, ICardEventPublisher eventPublisher)
    {
        int propertyCount = player.GetOwnedProperties().Count;
        if (propertyCount <= 0) return;
        int totalAmount = propertyCount * amount;
        eventPublisher.Publish(new ChargePlayerEvent(banker, totalAmount)); // TODO: This should charge the bank or banker player
        eventPublisher.Publish(new PayPlayerEvent(player, totalAmount));
    }

    private void payPerProperty(Player player, Player banker, ICardEventPublisher eventPublisher)
    {
        int propertyCount = player.GetOwnedProperties().Count;
        if (propertyCount <= 0) return;
        int totalAmount = propertyCount * amount;
        eventPublisher.Publish(new ChargePlayerEvent(player, totalAmount));
        eventPublisher.Publish(new PayPlayerEvent(banker, totalAmount)); // TODO: The bank or the banker player should receive this
    }

    private void collectFromAll(Player player, ICardEventPublisher eventPublisher)
    {
        // TODO: Implement logic to collect from all players
    }

    private void payToAll(Player player, ICardEventPublisher eventPublisher)
    {
        // TODO: Implement logic to pay all players
    }

    private void collectFromPlayer(Player player, Player targetPlayer, ICardEventPublisher eventPublisher)
    {
        eventPublisher.Publish(new ChargePlayerEvent(targetPlayer, amount));
        eventPublisher.Publish(new PayPlayerEvent(player, amount));
    }

    private void payToPlayer(Player player, Player targetPlayer, ICardEventPublisher eventPublisher)
    {
        eventPublisher.Publish(new ChargePlayerEvent(player, amount));
        eventPublisher.Publish(new PayPlayerEvent(targetPlayer, amount));
    }
}