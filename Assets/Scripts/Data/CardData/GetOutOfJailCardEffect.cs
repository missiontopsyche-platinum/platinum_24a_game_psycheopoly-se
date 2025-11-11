using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using UnityEngine;

public class GetOutOfJailCardEffect : CardEffect
{
    [SerializeField] public JailStateChangedEventChannel JailStateChangedEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;

        if (JailStateChangedEventChannel == null)
        {
            Logging.Logger.Error("GetOutOfJailCardEffect.ApplyEffect",
                "JailStateChangedEventChannel is not assigned in GetOutOfJailCardEffect.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        JailStateChangedEventChannel.RaiseEvent(new JailStateChangedEvent(player, false, 0));
    }
}
