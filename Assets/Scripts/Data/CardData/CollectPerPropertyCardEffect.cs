using Events.EventDataStructures;
using UnityEngine;

public class CollectPerPropertyCardEffect : CardEffect
{
    [SerializeField] public int Amount = 0;
    //[SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel; TODO: This must be refactored to target bank instead of player
    [SerializeField] public PayPlayerEventChannel payPlayerEventChannel;
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;
        int propertyCount = player.GetOwnedProperties().Count;
        if (propertyCount <= 0 || Amount <= 0) return;
        int totalAmount = propertyCount * Amount;
        // chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(banker, totalAmount)); TODO: This should charge the bank or banker player
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, totalAmount));
    }
}
