using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Purchase;
using UnityEngine;

//Adapter that exposes data from an OwnableSpaceData asset 
public class OwnableSpaceTileAdapter : MonoBehaviour, ITileRentInfo, IPurchasableTileInfo
{
    [SerializeField] private OwnableSpaceData data;

    //TODO will need to be completed within the second semster once more of the 
    //rulesets like color groups are added into the program
    
    public string Name => data ? data.spaceName : "Unnamed";

    public TileType Type => TileType.Street; 
    //TODO read from derived ScriptableObject like Property, Railroad, Utility

    public ColorGroup Group => ColorGroup.None;
    //TODO map once properties have color groups in ScriptableObjects

    public bool IsMortgaged => false; 
    //TODO map from data.collaborationValue if needed

    public int HouseCount => 0;
    //TODO map from PropertySpaceData upgrade levels

    public int BaseRent => 0;
    //TODO map from researchFundingValues[0]

    public int[] RentByHouses => null;
    //TODO map from researchFundingValues array

    public int PurchasePrice => data ? data.buyPrice : 0;
}
