using NUnit.Framework;
using PsycheOpoly.Board;
using System.Collections.Generic;
using Tests.EditMode;
using Tests.EditMode.PlayerControllerTests.Builders;
using UnityEngine;

public class PurchaseActionTestBase
{
    private readonly List<ScriptableObject> createdInstances = new();

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
}
