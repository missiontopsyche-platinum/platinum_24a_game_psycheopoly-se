namespace Assets.Scripts.Managers.Rent
{
    public enum TileType { Street, Railroad, Utility, Other }
    public enum ColorGroup { None, Brown, LightBlue, Pink, Orange, Red, Yellow, Green, DarkBlue }

    /// <summary>Minimal shape strategy needs. 
    /// Create adapters to connect real tile data.</summary>
    public interface ITileRentInfo
    {
        string Name { get; }
        TileType Type { get; }
        ColorGroup Group { get; }     //Street group
        bool IsMortgaged { get; }
        int HouseCount { get; }       
        int BaseRent { get; }         //street base rent
        int[] RentByHouses { get; }   
    }

    /// <summary>Rules/constants centralized to keep StandardRentStrategy clean.</summary>
    public interface IRuleSet
    {
        int RailroadBaseRent();       //usually 25 in game
        int UtilityRentSingleMult();  //usually 4 in game
        int UtilityRentBothMult();    //usually 10 in game
        int StreetsInGroup(ColorGroup g); //total street amount in a color set
    }

    /// <summary>Ownership lookups kept external to Strategy.</summary>
    public interface IOwnershipService
    {
        Player GetOwner(ITileRentInfo tile);
        int CountOwnedInGroup(Player owner, ColorGroup group);
        int CountRailroadsOwned(Player owner);
        bool OwnsBothUtilities(Player owner);
    }
}
