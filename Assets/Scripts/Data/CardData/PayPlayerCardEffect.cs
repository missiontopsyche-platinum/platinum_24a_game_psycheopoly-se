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
    [SerializeField] public PayPlayerEventChannel payPlayerEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;
        if (payPlayerEventChannel == null)
        {
            Logging.Logger.Error("PayPlayerCardEffect.ApplyEffect",
                "PayPlayerEventChannel is not assigned.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        Logging.Logger.Info("PayPlayerCardEffect.ApplyEffect",
            $"Effect: Paying {player.GetPName()} ${amount}.",
            Logging.LogCategory.Gameplay,
            this);
        
        payPlayerEventChannel.RaiseEvent(new PayPlayerEvent(player, amount));
        
    }
}
