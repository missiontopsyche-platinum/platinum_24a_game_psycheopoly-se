using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Causes the acting player to pay every other player a fixed amount.
///
/// Publishes a MoneyDistributionEvent, which PlayerManager uses
/// to distribute funds to opponents.
/// </summary>
[CreateAssetMenu(fileName = "PayAllPlayersCardEffect", menuName = "Card Data/Effects/PayAllPlayersCardEffect")]
public class PayAllPlayersCardEffect : CardEffect
{
    [SerializeField] public int Amount = 0;
    [SerializeField] public MoneyDistributionEventChannel moneyDistributionEventChannel;
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;
        moneyDistributionEventChannel.RaiseEvent(new MoneyDistributionEvent(
            MoneyDistributionEvent.MoneyDistributionEventType.Pay, player, Amount));
    }
}