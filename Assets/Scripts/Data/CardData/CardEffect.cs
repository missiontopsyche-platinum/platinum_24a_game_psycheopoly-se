[System.Serializable]
public abstract class CardEffect
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
