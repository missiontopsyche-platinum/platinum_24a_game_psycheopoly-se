using System;
using Events.EventDataStructures;
using UnityEngine;

public abstract class SpaceData : ScriptableObject
{
    [Header("Basic Data")]
    [SerializeField] public String spaceName;
    [SerializeField] public Color spaceColor;

    //Always visible wihtin the UI
    [SerializeField] public string shortDisplayName;
    [SerializeField] public Color groupColor = Color.white;
    [SerializeField] public Sprite smallIcon;

    [Header("Event Channels")] 
    [SerializeField] public SpaceHoverEventChannel spaceHoverEventChannel;
    [SerializeField] public BooleanEventChannel onSpaceExitEventChannel;

    [Header("Artwork")]
    [SerializeField] public Sprite artwork;

    public Sprite Artwork => artwork;

    public abstract void OnLanded(Player player);
    public abstract void OnPassed(Player player);

    public virtual SpaceHoverEvent OnHover()
    {
        var payload = new SpaceHoverEvent(spaceName, spaceColor);
        payload.AppendInformation($"Type: {GetType().Name}");

        return payload;
    }

    public string GetShortName()
    {
        if (!string.IsNullOrWhiteSpace(shortDisplayName))
            return shortDisplayName;
        return spaceName;
    }

    public void OnExit() => onSpaceExitEventChannel?.RaiseEvent(true);
}
