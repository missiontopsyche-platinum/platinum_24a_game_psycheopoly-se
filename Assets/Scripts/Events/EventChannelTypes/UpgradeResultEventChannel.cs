using UnityEngine;

public class UpgradeResultEventChannel : ScriptableObject
{
    public System.Action<UpgradeResultEvent> OnEventRaised;

    public void RaiseEvent(UpgradeResultEvent result)
    {
        OnEventRaised?.Invoke(result);
    }
}