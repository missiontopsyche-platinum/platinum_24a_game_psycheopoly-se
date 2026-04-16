using System;
using UnityEngine;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Events.EventDataStructures;

/// <summary>
/// Gives money directly to the acting player.
/// Raises a PayPlayerEvent with (player, amount).
/// </summary>
[CreateAssetMenu(fileName = "PayPlayerCardEffect", menuName = "Card Data/Effects/PayPlayerCardEffect")]

public class PayPlayerCardEffect : CardEffect
{
    [SerializeField] public int amount = 0;
    [SerializeField] public NoActionLandingEventChannel noActionLandingEventChannel;
    [SerializeField] public ChargePlayerEventChannel chargePlayerEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;

        if (amount >= 0)
        {
            // directly add money to player and trigger a notification UI to signal
            // resolution complete
            player.AddMoney(amount);
            noActionLandingEventChannel.RaiseEvent(new NoActionLandingEvent(
                "Money Added!",
                $"You gained ${amount}!"));
        }
        else
        {
            // charge money to the player, which handles bankruptcy checks and a
            // UI notification + resolution completion
            chargePlayerEventChannel.RaiseEvent(
                new ChargePlayerEvent(player, Mathf.Abs(amount)));
        }
    }
}
