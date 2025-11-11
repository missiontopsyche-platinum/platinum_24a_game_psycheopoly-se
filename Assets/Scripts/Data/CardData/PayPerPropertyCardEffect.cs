using Events.EventDataStructures;
using UnityEngine;

public class PayPerPropertyCardEffect : CardEffect
{
    [SerializeField] public int Amount = 0;
    [SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel;
    // [SerializeField] public PayPlayerEventChannel payPlayerEventChannel; TODO: This must be refactored to target bank instead of player
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;
        int propertyCount = player.GetOwnedProperties().Count;
        if (propertyCount <= 0 || Amount <= 0) return;
        int totalAmount = propertyCount * Amount;
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(player, totalAmount));
        // payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, totalAmount)); TODO: This should pay the bank or banker player
    }
}
