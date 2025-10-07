using UnityEngine;
using System;
using System.Collections.Generic;

public class EventChannel<T> : ScriptableObject
{
    private readonly List<Action<T>> listeners = new List<Action<T>>();

    public void Subscribe(Action<T> listener)
    {
        if (listener == null)
        {
            Debug.LogWarning("Attempted to subscribe null listener on " + this.name);
            return;
        }
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void Unsubscribe(Action<T> listener)
    {
        if (listener == null)
            return;
        listeners.Remove(listener);
    }

    public void RaiseEvent(T data)
    {
        foreach (Action<T> listener in listeners)
        {
            listener?.Invoke(data);
        }
    }

    public void ClearAllListeners()
    {
        listeners.Clear();
    }
}
