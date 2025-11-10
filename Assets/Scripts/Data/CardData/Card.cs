using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Card Data/Card")]
public class Card : ScriptableObject
{
    public string title;
    public string bodyText;

    [SerializeReference]
    public List<CardEffect> effect;
}
