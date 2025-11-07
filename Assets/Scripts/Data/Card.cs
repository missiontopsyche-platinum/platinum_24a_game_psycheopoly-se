using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/Card")]
public class Card : ScriptableObject
{
    public string title;
    public string bodyText;

    [SerializeReference]
    List<CardEffect> effects;
}
