using Assets.Scripts.Managers.Rent;

namespace Assets.Scripts.Managers.Purchase
{
    //Extends the rent view of a tile with purchase-specific data
    //Implemented by adapters that wrap OwnableSpaceData
    public interface IPurchasableTileInfo : ITileRentInfo
    {
        int PurchasePrice { get; }
    }
}
