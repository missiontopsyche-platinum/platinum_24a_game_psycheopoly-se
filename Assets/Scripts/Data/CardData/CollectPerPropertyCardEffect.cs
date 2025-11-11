using Events.EventDataStructures;
using UnityEngine;

public class CollectPerPropertyCardEffect : CardEffect
{
    [SerializeField] public int ChargeForHouse = 0;
    [SerializeField] public int ChargeForHotel = 0;
    //[SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel; TODO: This must be refactored to target bank instead of player
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
        // chargePlayerEventChannel.RaiseEvent(new ChargePlayerEvent(banker, totalAmount)); TODO: This should charge the bank or banker player
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, totalAmount));
    }
}
