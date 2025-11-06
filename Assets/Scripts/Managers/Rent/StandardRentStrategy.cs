using System;
using UnityEngine;

namespace Assets.Scripts.Managers.Rent
{
    /// <summary>Monopoly stype rent computation. </summary>
    public class StandardRentStrategy : IRentStrategy
    {
        public int ComputeRent(ITileRentInfo tile, Player owner, int diceTotal, IOwnershipService own, IRuleSet rules)
        {
            if (tile == null || owner == null) return 0;
            if (tile.IsMortgaged) return 0;

            switch (tile.Type)
            {
                case TileType.Street:
                    return StreetRent(tile, owner, own, rules);
                case TileType.Railroad:
                    return RailroadRent(owner, own, rules);
                case TileType.Utility: 
                    return UtilityRent(owner, diceTotal, own, rules);
                default: 
                    return 0; 
            }
        }

        private int StreetRent(ITileRentInfo tile, Player owner, IOwnershipService own, IRuleSet rules)
        {
            int houses = Mathf.Clamp(tile.HouseCount, 0, tile.RentByHouses?.Length > 0 ? tile.RentByHouses.Length - 1 : 0);

            //Houses and hotels table if needed will be checked first
            if (houses > 0 && tile.RentByHouses != null && tile.RentByHouses.Length > houses)
            {
                return tile.RentByHouses[houses];
            }

            //If there are no houses check monoploy which doubles base rent
            bool hasMonopoly = own.CountOwnedInGroup(owner, tile.Group) >= rules.StreetsInGroup(tile.Group);
            return hasMonopoly ? tile.BaseRent * 2 : tile.BaseRent;
        }

        private int RailroadRent(Player owner, IOwnershipService own, IRuleSet rules)
        {
            int n = Mathf.Clamp(own.CountRailroadsOwned(owner), 0, 4);
            if (n <= 0) return 0;

            //Doubles based on how many owned
            int baseRent = rules.RailroadBaseRent();
            int rent = baseRent << (n - 1);
            return rent;
        }

        private int UtilityRent(Player owner, int diceTotal, IOwnershipService own, IRuleSet rules)
        {
            if (diceTotal < 0) diceTotal = 0;
            bool ownsBoth = own.OwnsBothUtilities(owner);
            int mult = ownsBoth ? rules.UtilityRentBothMult() : rules.UtilityRentSingleMult();
            return diceTotal * mult; 
        }

    }




}