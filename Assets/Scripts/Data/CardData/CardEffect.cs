using UnityEngine;

/// <summary>
/// Abstract base class for all card effects.
/// A CardEffect represents a single unit of gameplay behavior
/// that can be combined with other effects to form a complete card.
/// </summary>
[System.Serializable]
public abstract class CardEffect : ScriptableObject
{
    public abstract void ApplyEffect(Player player);

    protected bool isValidPlayer(Player player)
    {
        if (player == null)
        {
            Logging.Logger.Error("CardEffect.IsValidPlayer",
                "Player context is null in CardEffect.",
                Logging.LogCategory.Gameplay,
                this);
            return false;
        }
        return true;
    }
}
