using UnityEngine;
using UnityEngine.UI;
using Logging;
using System;
using System.Collections.Generic;

// The base class for the HUD elements. Might be important for later
// if we want to add shared traits like font style, size, color, etc.
// US167-T169
public class UIPanelBase : MonoBehaviour
{
    // This is just to make sure components are unsubscribed
    // on disable.
    private readonly List<Action> unsubscribers = new();
    protected void Subscribe<T>(EventChannel<T> channel, Action<T> listener)
    {
        if (channel == null || listener == null) return;
        channel.Subscribe(listener);
        // Adding the unsubscriber to the list
        unsubscribers.Add(() => channel.Unsubscribe(listener));
    }
    
    // Unsubscribers are cleared out when the component is disabled.
    protected void ClearSubscriptions()
    {
        foreach (Action unsubscriber in unsubscribers)
            unsubscriber?.Invoke();
        unsubscribers?.Clear();
    }
    // Helper to safely format text.
    protected void SetTextSafe(Text text, string input)
    {
        if (!text)
        {
            Logging.Logger.Warn("UIPanelBase.SetTextSafe",
                $"Text: {nameof(text)} component is null, cannot set text.",
                LogCategory.UI,
                this);
            return;
        }
        text.text = input ?? "";
    }
}
