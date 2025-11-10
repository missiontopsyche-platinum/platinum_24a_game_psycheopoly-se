using UnityEngine;

public class MoveCardEffect : CardEffect
{
    public enum EffectType
    {
        MoveForward,
        MoveBackward,
        MoveToSpace,
        MoveToNearestPropertyType,
    }
    [SerializeField] EffectType effectType;
    [SerializeField] int spacesToMove = 0; // Used to move player forward or backward
    [SerializeField] int targetSpaceIndex = -1; // Used to move player to specific space

    public override void ApplyEffect(CardEffectContext context)
    {
        ICardEventPublisher eventPublisher = context.EventPublisher;
        Player player = context.player;

        switch (effectType)
        {
            case EffectType.MoveForward:
                eventPublisher.Publish(new MovePlayerEvent(player.GetId(), spacesToMove));
                break;
            case EffectType.MoveBackward:
                eventPublisher.Publish(new MovePlayerEvent(player.GetId(), -spacesToMove));
                break;
            case EffectType.MoveToSpace:
                // TODO: Implement logic to move player to a specific space index
                break;
            case EffectType.MoveToNearestPropertyType:
                // TODO: Implement logic to find nearest property type, get its index, and move player there
                break;
            default:
                Logging.Logger.Warn("MoveCardEffect.ApplyEffect",
                    "Unknown EffectType in MoveCardEffect: " + effectType.ToString(),
                    Logging.LogCategory.Gameplay,
                    this);
                break;
        }
    }
}
