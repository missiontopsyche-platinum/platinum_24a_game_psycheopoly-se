using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Card", menuName = "Card Data/Card")]
public class Card : ScriptableObject
{
    public string title;
    public string bodyText;
    //This may be changed later on if we decide not to use images
    //On all of the cards but for now I added it since it is easier
    //To delete later than try to add it
    public Sprite artwork; 

    [SerializeReference]
    public List<CardEffect> effect;
}
