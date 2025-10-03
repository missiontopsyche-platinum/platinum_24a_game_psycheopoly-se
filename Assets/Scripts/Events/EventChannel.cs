using UnityEngine;
using System;

public class EventChannel<T> : ScriptableObject
{
    private event Action<T> OnEventRaised;

    public void Subscribe(Action<T> listener) {}
    public void Unsubscribe(Action<T> listener) {}
    public void RaiseEvent(T data) {}
    public void ClearAllListeners() {}
}
