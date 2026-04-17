using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// Sends the player directly to jail for a configured number of turns.
/// </summary>
[CreateAssetMenu(fileName = "GoToJailCardEffect", menuName = "Card Data/Effects/GoToJailCardEffect")]
public class GoToJailCardEffect : CardEffect
{
    [SerializeField] public int TurnsInJail = 1;
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

        JailStateChangedEventChannel.RaiseEvent(new JailStateChangedEvent(player, true, TurnsInJail));
    }
}

