using UnityEngine;

public class PayAllPlayersCardEffect : CardEffect
{
    [SerializeField] public int Amount = 0;
    [SerializeField] public MoneyDistributionEventChannel payAllPlayersEventChannel;
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;
        payAllPlayersEventChannel.RaiseEvent(new MoneyDistributionEvent(player, Amount));
    }
}