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
    [SerializeField] public EffectType Type;
    [SerializeField] public int SpacesToMove = 0; // Used to move player forward or backward
    [SerializeField] public int TargetSpaceIndex = -1; // Used to move player to specific space
    [SerializeField] public MovePlayerEventChannel movePlayerEventChannel;

    public override void ApplyEffect(CardEffectContext context)
    {
        if (!IsValidContext(context)) return;

        Player player = context.player;

        switch (Type)
        {
            case EffectType.MoveForward:
                movePlayerEventChannel.RaiseEvent(new MovePlayerEvent(player.GetId(), SpacesToMove));
                break;
            case EffectType.MoveBackward:
                movePlayerEventChannel.RaiseEvent(new MovePlayerEvent(player.GetId(), -SpacesToMove));
                break;
            case EffectType.MoveToSpace:
                // TODO: Implement logic to move player to a specific space index
                break;
            case EffectType.MoveToNearestPropertyType:
                // TODO: Implement logic to find nearest property type, get its index, and move player there
                break;
            default:
                Logging.Logger.Warn("MoveCardEffect.ApplyEffect",
                    "Unknown EffectType in MoveCardEffect: " + Type.ToString(),
                    Logging.LogCategory.Gameplay,
                    this);
                break;
        }
    }
}
