using UnityEngine;

public class CollectFromAllPlayersCardEffect : CardEffect
{
    [SerializeField] public int Amount = 0;
    [SerializeField] public MoneyDistributionEventChannel collectFromAllPlayersEventChannel;
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;
        collectFromAllPlayersEventChannel.RaiseEvent(new MoneyDistributionEvent(player, Amount));
    }
}

