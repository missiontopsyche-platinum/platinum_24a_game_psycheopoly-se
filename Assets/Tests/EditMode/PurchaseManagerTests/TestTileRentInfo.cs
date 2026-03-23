using Assets.Scripts.Managers.Rent;
using Assets.Scripts.Managers.Purchase;

namespace Tests.EditMode.PurchaseManagerTests
{
    internal class TestTileRentInfo : ITileRentInfo
    {
        public string Name        { get; set; }
        public TileType Type      { get; set; }
        public ColorGroup Group   { get; set; }
        public bool IsMortgaged   { get; set; }
        public int HouseCount     { get; set; }
        public int BaseRent       { get; set; }
        public int[] RentByHouses { get; set; }

        //Purchase specific data 
        public int PurchasePrice  { get; set; }
    }
}
