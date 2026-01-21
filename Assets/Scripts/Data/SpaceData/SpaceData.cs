using System;
using Events.EventDataStructures;
using UnityEngine;

public abstract class SpaceData : ScriptableObject
{
    [Header("Basic Data")]
    [SerializeField] public String spaceName;
    [SerializeField] public Color spaceColor;

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
        return new SpaceHoverEvent(spaceName, spaceColor);
    }

    public void OnExit() => onSpaceExitEventChannel?.RaiseEvent(true);
}
