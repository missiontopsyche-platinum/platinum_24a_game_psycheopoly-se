using UnityEngine;

public class MoveCardEffect : CardEffect
{
    public enum EffectType
    {
        MoveForward,
        MoveBackward
    }
    [SerializeField] public EffectType Type;
    [SerializeField] public int SpacesToMove = 1;
    [SerializeField] public MovePlayerEventChannel MovePlayerEventChannel;

    public override void ApplyEffect(Player player)
    {
        if (!isValidPlayer(player)) return;

        if (MovePlayerEventChannel == null)
        {
            Logging.Logger.Error("MoveCardEffect.ApplyEffect",
                "MovePlayerEventChannel is not assigned.",
                Logging.LogCategory.Gameplay,
                this);
            return;
        }

        switch (Type)
        {
            case EffectType.MoveForward:
                MovePlayerEventChannel.RaiseEvent(new MovePlayerEvent(player.GetId(), SpacesToMove));
                break;
            case EffectType.MoveBackward:
                MovePlayerEventChannel.RaiseEvent(new MovePlayerEvent(player.GetId(), -SpacesToMove));
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
