using UnityEngine;

[System.Serializable]
public abstract class CardEffect
{
    public abstract void ApplyEffect<T>(Player player, EventChannel<T> eventChannel);
}
