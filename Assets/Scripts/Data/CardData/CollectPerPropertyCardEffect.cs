using Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// Pays the acting player based on the number of houses and hotels owned.
///
/// Upgrade levels:
/// - 1–4 = houses
/// - 5   = hotel
///
/// Total:
/// (ChargeForHouse × houses) + (ChargeForHotel × hotels)
///
/// Publishes a PayPlayerEvent to deposit funds.
/// </summary>
[CreateAssetMenu(fileName = "CollectPerPropertyCardEffect", menuName = "Card Data/Effects/CollectPerPropertyCardEffect")]
public class CollectPerPropertyCardEffect : CardEffect
{
    [SerializeField] public int ChargeForHouse = 0;
    [SerializeField] public int ChargeForHotel = 0;
    [SerializeField] public PayPlayerEventChannel payPlayerEventChannel;
    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player) 
            || player.GetOwnedProperties().Count <= 0 
            || (ChargeForHouse <= 0 && ChargeForHotel <= 0)) return;

        int houseCount = 0;
        int hotelCount = 0;

        foreach (var property in player.GetOwnedProperties())
        {
            if (property is PropertySpaceData)
            {
                PropertySpaceData propertySpace = (PropertySpaceData)property;
                if (propertySpace.GetCurrentUpgradeLevel() == 0) continue;
                if (propertySpace.GetCurrentUpgradeLevel() < 5) houseCount++;
                else hotelCount++;
            }
        }
        int totalAmount = (ChargeForHouse * houseCount) + (ChargeForHotel * hotelCount);
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, totalAmount));
    }
}
