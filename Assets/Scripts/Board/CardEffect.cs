using UnityEngine;

[System.Serializable]
public abstract class CardEffect<T>
{
    public abstract void ApplyEffect(CardContext<T> context);
}
