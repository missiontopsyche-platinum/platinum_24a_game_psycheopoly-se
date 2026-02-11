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
    [SerializeField] public int numberOfPropertiesInGroup = 0;
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
        var payload = new SpaceHoverEvent(this);
        payload.AppendInformation($"Type: {GetType().Name}");

        // THIS SHOULD NOT BE CALLED HERE - the derived classes *must* override this and attach their
        // special information.
        // spaceHoverEventChannel?.RaiseEvent(payload); 
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
