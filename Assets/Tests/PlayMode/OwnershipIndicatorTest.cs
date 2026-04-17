using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

public class OwnershipIndicatorTests
{
    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        Assert.IsNotNull(obj, $"Cannot set field '{fieldName}' because target object is null.");
        var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found on {obj.GetType().Name}");
        field.SetValue(obj, value);
    }

    private static SpaceRenderer CreateSpaceRendererGO(string name)
    {
        var go = new GameObject(name);
        go.AddComponent<BoxCollider>();

        var renderer = go.AddComponent<SpaceRenderer>();
        Assert.IsNotNull(renderer, "Failed to add SpaceRenderer component.");

        return renderer;
    }

    [UnityTest]
    public IEnumerator OwnedIcon_ShowsOnlyWhenPropertyOwned()
    {
        var renderer = CreateSpaceRendererGO("SpaceRenderer_Test");

        var ownedIcon = new GameObject("OwnedIcon");
        ownedIcon.SetActive(true);

        var mortgagedIcon = new GameObject("MortgagedIcon");
        mortgagedIcon.SetActive(true);

        SetPrivateField(renderer, "ownedIconGO", ownedIcon);
        SetPrivateField(renderer, "mortgagedIconGO", mortgagedIcon);

        var property = ScriptableObject.CreateInstance<PropertySpaceData>();
        property.spaceName = "Test Property";

        // originally unowned
        renderer.SetUpSpace(property, 1f);

        //assert 1
        Assert.IsFalse(ownedIcon.activeSelf, "Owned icon should be hidden when owner is null.");
        Assert.IsFalse(mortgagedIcon.activeSelf, "Mortgaged icon should be hidden when not mortgaged.");

        // now owned
        var player = ScriptableObject.CreateInstance<Player>();
        player.SetPName("Tester");
        property.SetOwner(player);

        renderer.SetUpSpace(property, 1f);

        // assert 2
        Assert.IsTrue(ownedIcon.activeSelf, "Owned icon should be visible when owner is set.");
        Assert.IsFalse(mortgagedIcon.activeSelf, "Mortgaged icon should be hidden when not mortgaged.");

        // cleanup
        Object.DestroyImmediate(renderer.gameObject);
        Object.DestroyImmediate(property);
        Object.DestroyImmediate(player);

        yield return null;
    }

    [UnityTest]
    public IEnumerator MortgagedIcon_ShowsOnlyWhenPropertyMortgaged()
    {
        // Arrange
        var renderer = CreateSpaceRendererGO("SpaceRenderer_Mortgage_Test");

        var ownedIcon = new GameObject("OwnedIcon");
        ownedIcon.SetActive(true);

        var mortgagedIcon = new GameObject("MortgagedIcon");
        mortgagedIcon.SetActive(true);

        SetPrivateField(renderer, "ownedIconGO", ownedIcon);
        SetPrivateField(renderer, "mortgagedIconGO", mortgagedIcon);

        var property = ScriptableObject.CreateInstance<PropertySpaceData>();
        property.spaceName = "Test Property";

        var player = ScriptableObject.CreateInstance<Player>();
        player.SetPName("Tester");
        property.SetOwner(player);

        // start as owned && NOT mortgaged
        property.isMortgaged = false;
        renderer.SetUpSpace(property, 1f);

        Assert.IsTrue(ownedIcon.activeSelf, "Owned icon should be visible when owned and not mortgaged.");
        Assert.IsFalse(mortgagedIcon.activeSelf, "Mortgaged icon should be hidden when not mortgaged.");

        // Now mortgaged
        property.isMortgaged = true;
        renderer.SetUpSpace(property, 1f);

        Assert.IsFalse(ownedIcon.activeSelf, "Owned icon should be hidden when property is mortgaged.");
        Assert.IsTrue(mortgagedIcon.activeSelf, "Mortgaged icon should be visible when property is mortgaged.");

        // cleanup
        Object.DestroyImmediate(renderer.gameObject);
        Object.DestroyImmediate(property);
        Object.DestroyImmediate(player);

        yield return null;
    }

    [UnityTest]
    public IEnumerator Icons_HiddenForNonPropertySpaces()
    {
        var renderer = CreateSpaceRendererGO("SpaceRenderer_Test2");

        var ownedIcon = new GameObject("OwnedIcon");
        ownedIcon.SetActive(true);

        var mortgagedIcon = new GameObject("MortgagedIcon");
        mortgagedIcon.SetActive(true);

        SetPrivateField(renderer, "ownedIconGO", ownedIcon);
        SetPrivateField(renderer, "mortgagedIconGO", mortgagedIcon);

        var nonProperty = ScriptableObject.CreateInstance<DummySpaceData>();
        nonProperty.spaceName = "Dummy";

        renderer.SetUpSpace(nonProperty, 1f);

        Assert.IsFalse(ownedIcon.activeSelf, "Owned icon should be hidden for non-property spaces.");
        Assert.IsFalse(mortgagedIcon.activeSelf, "Mortgaged icon should be hidden for non-property spaces.");

        Object.DestroyImmediate(renderer.gameObject);
        Object.DestroyImmediate(nonProperty);

        yield return null;
    }

    private class DummySpaceData : SpaceData
    {
        public override void OnLanded(Player player) { }
        public override void OnPassed(Player player) { }
    }
}