using UnityEngine;

/// <summary>
/// Causes all other players to pay a fixed amount to the acting player.
/// Publishes a MoneyDistributionEvent processed by PlayerManager.
/// </summary>
[CreateAssetMenu(fileName = "CollectFromAllPlayersCardEffect", menuName = "Card Data/Effects/CollectFromAllPlayersCardEffect")]
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

