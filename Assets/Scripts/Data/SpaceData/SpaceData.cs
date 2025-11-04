using System;
using UnityEngine;

public abstract class SpaceData : ScriptableObject
{
    [SerializeField] public String spaceName;
    [SerializeField] public Color spaceColor;

    public abstract void OnLanded();
    public abstract void OnHover();
    public abstract void OnPassed();
}
