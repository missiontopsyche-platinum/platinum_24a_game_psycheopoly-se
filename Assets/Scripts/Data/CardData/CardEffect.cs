using UnityEngine;

[System.Serializable]
public abstract class CardEffect
{
    public abstract void ApplyEffect(CardEffectContext context);
}
