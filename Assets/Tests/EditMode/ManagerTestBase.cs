using System.Collections.Generic;
using UnityEngine;

public class ManagerTestBase
{
    protected List<ScriptableObject> eventChannels = new();

    protected T CreateChannel<T>() where T : ScriptableObject
    {
        T channel = ScriptableObject.CreateInstance<T>();
        eventChannels.Add(channel);
        return channel;
    }
    
    protected void DestroyTestObjects(params UnityEngine.Object[] objects)
    {
        // destroy all passed objects
        foreach (var obj in objects)
            if (obj != null)
                Object.DestroyImmediate(obj);
        
        // destroy all event channels
        foreach (var channel in eventChannels)
            if (channel != null)
                Object.DestroyImmediate(channel);
        
        eventChannels.Clear();
    }
}
