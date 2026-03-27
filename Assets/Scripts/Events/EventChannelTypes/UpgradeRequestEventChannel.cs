using UnityEngine;

public class UpgradeRequestEventChannel : ScriptableObject
{
    public System.Action<UpgradeRequestEvent> OnEventRaised;

    public void RaiseEvent(UpgradeRequestEvent request)
    {
        OnEventRaised?.Invoke(request);
    }
}