using Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// Charges the acting player based on the number of houses and hotels owned.
///
/// Upgrade levels:
/// - 1–4 = houses
/// - 5   = hotel
///
/// Total:
/// (ChargeForHouse × houses) + (ChargeForHotel × hotels)
///
/// Publishes a ChargePlayerEvent to withdraw funds.
/// </summary>
[CreateAssetMenu(fileName = "PayPerPropertyCardEffect", menuName = "Card Data/Effects/PayPerPropertyCardEffect")]
public class PayPerPropertyCardEffect : CardEffect
{
    [SerializeField] public int ChargeForHouse = 0;
    [SerializeField] public int ChargeForHotel = 0;
    [SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel;
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
        chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(player, totalAmount));
    }
}
