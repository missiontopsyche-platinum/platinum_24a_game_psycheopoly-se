using System.Collections.Generic;
using NUnit.Framework;
using Tests.EditMode.PlayerControllerTests.Builders;
using UnityEngine;

public class MortgageTestBase 
{
    private readonly List<ScriptableObject> createdInstances = new List<ScriptableObject>();

    protected MockPlayerBuilder APlayer() => new MockPlayerBuilder();
    protected MockOwnableBuilder AnOwnableSpace() => new MockOwnableBuilder();

    protected T Track<T>(T instance) where T : ScriptableObject
    {
        createdInstances.Add(instance);
        return instance;
    }

    [TearDown]
    protected virtual void TearDown()
    {
        foreach (var instance in createdInstances)
            Object.DestroyImmediate(instance);
        createdInstances.Clear();
    }
}
