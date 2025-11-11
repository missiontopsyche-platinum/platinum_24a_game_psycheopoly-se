using UnityEngine;

[System.Serializable]
public abstract class CardEffect
{
    public abstract void ApplyEffect(CardEffectContext context);

    protected bool IsValidContext(CardEffectContext context)
    {
        if (context == null)
        {
            Logging.Logger.Warn("MoveCardEffect.ApplyEffect",
                "CardEffectContext is null.",
                Logging.LogCategory.Gameplay,
                this);
            return false;
        }

        if (context.player == null)
        {
            Logging.Logger.Warn("MoveCardEffect.ApplyEffect",
                "Player is null in CardEffectContext.",
                Logging.LogCategory.Gameplay,
                this);
            return false;
        }

        return true;
    }
}
