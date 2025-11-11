using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using Logging;
using UnityEngine;

public class JailCardEffect : CardEffect
{
    public enum EffectType
    {
        GoToJail,
        ReleaseFromJail
    }
    [SerializeField] public int TurnsInJail = 0;
    [SerializeField] public EffectType Type { get; }
    [SerializeField] public JailStateChangedEventChannel JailStateChangedEventChannel;
    public override void ApplyEffect(CardEffectContext context)
    {
        if (!IsValidContext(context)) return;

        Player player = context.player;

        switch (Type)
        {
            case EffectType.GoToJail:
                JailStateChangedEventChannel.RaiseEvent(new JailStateChangedEvent(player, true, TurnsInJail));
                break;
            case EffectType.ReleaseFromJail:
                JailStateChangedEventChannel.RaiseEvent(new JailStateChangedEvent(player, false, 0));
                break;
            default:
                Logging.Logger.Warn("JailCardEffect.ApplyEffect", 
                    "Unknown EffectType in JailCardEffect: " + Type.ToString(), 
                    Logging.LogCategory.Gameplay,
                    this);
                break;
        }

    }
}
