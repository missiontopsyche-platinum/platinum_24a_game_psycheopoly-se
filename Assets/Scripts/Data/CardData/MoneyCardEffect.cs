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
        CollectFromAllPlayers,
        PayToAllPlayers,
        // The bottom might not be necessary, but just in case
        CollectFromPlayer,
        PayToPlayer
    };
    [SerializeField] public EffectType effectType;
    [SerializeField] public int amount;
    [SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel;
    [SerializeField] public PayPlayerEventChannel payPlayerEventChannel;

    public override void ApplyEffect(CardEffectContext context)
    {
        if (!IsValidContext(context)) return;

        Player player = context.player;
        Player targetPlayer = context.targetPlayer;

        // TODO: We need a centralized banking/money system to handle banking transactions
        switch (effectType)
        {
            case EffectType.CollectFromBank:
                CollectFromBank(player, targetPlayer);
                break;
            case EffectType.PayToBank:
                PayToBank(player, targetPlayer);
                break;
            case EffectType.CollectPerProperty:
                CollectPerProperty(player, targetPlayer);
                break;
            case EffectType.PayPerProperty:
                PayPerProperty(player, targetPlayer);
                break;
            case EffectType.CollectFromAllPlayers:
                // TODO: Implement
                break;
            case EffectType.PayToAllPlayers:
                // TODO: Implement
                break;
            case EffectType.CollectFromPlayer:
                collectFromPlayer(player, targetPlayer);
                break;
            case EffectType.PayToPlayer:
                payToPlayer(player, targetPlayer);
                break;
            default:
                Logging.Logger.Warn("MoneyCardEffect.ApplyEffect",
                    $"Unknown EffectType: {effectType}",
                    Logging.LogCategory.Gameplay,
                    this);
                break;
        }
    }

    private void CollectFromBank(Player player, Player banker)
    {
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(banker, amount)); // TODO: This should charge the bank or banker player
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, amount));
    }

    private void PayToBank(Player player, Player banker)
    {
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(player, amount));
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(banker, amount)); // TODO: The bank or the banker player should receive this
    }

    private void CollectPerProperty(Player player, Player banker)
    {
        int propertyCount = player.GetOwnedProperties().Count;
        if (propertyCount <= 0) return;
        int totalAmount = propertyCount * amount;
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(banker, totalAmount)); // TODO: This should charge the bank or banker player
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, totalAmount));
    }

    private void PayPerProperty(Player player, Player banker)
    {
        int propertyCount = player.GetOwnedProperties().Count;
        if (propertyCount <= 0) return;
        int totalAmount = propertyCount * amount;
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(player, totalAmount));
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(banker, totalAmount)); // TODO: The bank or the banker player should receive this
    }

    private void CollectFromAllPlayers(Player player)
    {
        // TODO: Implement logic to collect from all players
    }

    private void PayToAllPlayers(Player player)
    {
        // TODO: Implement logic to pay all players
    }

    private void collectFromPlayer(Player player, Player targetPlayer)
    {
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(targetPlayer, amount));
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, amount));
    }

    private void payToPlayer(Player player, Player targetPlayer)
    {
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(player, amount));
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(targetPlayer, amount));
    }
}