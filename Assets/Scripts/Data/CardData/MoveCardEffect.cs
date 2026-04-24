using Events.EventDataStructures;
using UnityEngine;

/// <summary>
/// Moves a player forward or backward a fixed number of spaces.
/// </summary>
[CreateAssetMenu(fileName = "MoveCardEffect", menuName = "Card Data/Effects/MoveCardEffect")]
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
    [SerializeField] public NoActionLandingEventChannel NoActionLandingEventChannel;

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
                Logging.Logger.Error("MoveCardEffect.ApplyEffect",
                    "Unknown EffectType in MoveCardEffect: " + Type.ToString(),
                    Logging.LogCategory.Gameplay,
                    this);
                return;
        }
        // there is no action taken from this card, it doesn't say to pay rent- just move back three
        // spaces... so I'll assume that no action is to be taken. hdathert
        NoActionLandingEventChannel.RaiseEvent(new NoActionLandingEvent(
            "Move Effect!",
            $"{player.GetPName()} moved {(Type == EffectType.MoveForward ? "forward" : "backward" )} " +
            $"{SpacesToMove} spaces!"));
    }
}
