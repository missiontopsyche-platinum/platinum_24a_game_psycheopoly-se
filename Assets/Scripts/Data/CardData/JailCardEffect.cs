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
    [SerializeField] public int turnsInJail = 0;
    [SerializeField] public EffectType effectType;
    public override void ApplyEffect(CardEffectContext context)
    {
        Player player = context.player;
        ICardEventPublisher eventPublisher = context.EventPublisher;

        switch (effectType)
        {
            case EffectType.GoToJail:
                eventPublisher.Publish(new JailStateChangedEvent(player, true, turnsInJail));
                break;
            case EffectType.ReleaseFromJail:
                eventPublisher.Publish(new JailStateChangedEvent(player, false, 0));
                break;
            default:
                Logging.Logger.Warn("JailCardEffect.ApplyEffect", 
                    "Unknown EffectType in JailCardEffect: " + effectType.ToString(), 
                    Logging.LogCategory.Gameplay,
                    this);
                break;
        }

    }
}
