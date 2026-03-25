using Assets.Scripts.Managers.Purchase;
using Assets.Scripts.Managers.Rent;
using System;
using System.Linq;
using UnityEngine;

//Adapter that exposes data from an OwnableSpaceData asset 

public class OwnableSpaceTileAdapter : MonoBehaviour, ITileRentInfo, IPurchasableTileInfo
{
    [SerializeField] private OwnableSpaceData data;
    public OwnableSpaceData Data => data;

    //TODO will need to be completed within the second semster once more of the 
    //rulesets like color groups are added into the program

    public string Name => data ? data.spaceName : "Unnamed";

    public TileType Type => TileType.Street;
    //TODO read from derived ScriptableObject like Property, Railroad, Utility

    public ColorGroup Group => ColorGroup.None;
    //TODO map once properties have color groups in ScriptableObjects

    public bool IsMortgaged => data != null && data.isMortgaged;
    //TODO map from data.collaborationValue if needed

    public int HouseCount => UpgradeLevel;
    public int BaseRent =>
        RentByHouses != null && RentByHouses.Length > 0
        ? RentByHouses[0] : 0;
    public int[] RentByHouses => data != null && data is PropertySpaceData pd ? 
        pd.researchFundingValues : null;
    public int PurchasePrice => data != null ? 
        data.buyPrice : 0;
    public int UpgradeCost => data is PropertySpaceData pd ? 
        pd.dataPointCost : 0;
    public int MaxUpgradeLevel =>
        data is PropertySpaceData pd ? 
        pd.MaxUpgradeLevel : 0;
    public int UpgradeLevel => 
        data is PropertySpaceData pd ? 
        pd.GetCurrentUpgradeLevel() : 0;
    public bool IsMaxed =>
        data is PropertySpaceData pd && pd.IsMaxed;
    public int[] UpgradeCostByLevel =>
    data is PropertySpaceData pd
        ? pd.UpgradeCostByLevel
        : Array.Empty<int>();

    public int GetNextUpgradeCost()
    {
        return data is PropertySpaceData pd
            ? pd.GetNextUpgradeCost()
            : 0;
    }

    public void ApplyUpgrade()
    {
        if (data is PropertySpaceData pd)
            pd.TryUpgrade();
    }
}
