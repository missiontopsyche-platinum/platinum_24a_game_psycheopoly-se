using System;
using UnityEngine;

public abstract class OwnableSpaceData : SpaceData
{ 
    [SerializeField] public int collaborationValue;
    private Player owner;
}
