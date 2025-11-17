using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single Chance or Community Chest style card.
/// Each card contains UI text and an ordered list of CardEffects
/// that execute sequentially when the card is drawn.
/// </summary>
[CreateAssetMenu(fileName = "Card", menuName = "Card Data/Card")]
public class Card : ScriptableObject
{
    public string title;
    public string bodyText;

    [SerializeReference]
    public List<CardEffect> effect;
}
