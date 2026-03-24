using System;
using UnityEngine;
using Logging;
using Assets.Scripts.Managers.Rules;

namespace Assets.Scripts.Managers.Rent
{

    public static class RentCalculator
    {
        public static int ComputeRent(
                    ITileRentInfo tile,
                    Player owner,
                    int diceTotal,
                    IOwnershipService ownership,
                    IRuleSet rules)
                {
                    if (tile == null || owner == null || ownership == null || rules == null)
                        return 0;

                    //checks if property is mortgaged
                    if (tile.IsMortgaged)
                        return 0;

                    switch (tile.Type)
                    {
                        case TileType.Street:
                            return StreetRent(tile, owner, ownership, rules);

                        case TileType.Railroad:
                            return RailroadRent(owner, ownership, rules);

                        case TileType.Utility:
                            return UtilityRent(owner, diceTotal, ownership, rules);

                        default:
                            return 0;
                    }
                }

                public static int StreetRent(
                    ITileRentInfo tile,
                    Player owner,
                    IOwnershipService ownership,
                    IRuleSet rules)
                {
                    if (tile == null || owner == null || ownership == null || rules == null)
                        return 0;

                    int maxIndex = tile.RentByHouses != null && tile.RentByHouses.Length > 0
                        ? tile.RentByHouses.Length - 1
                        : 0;

                    int houses = Mathf.Clamp(tile.HouseCount, 0, maxIndex);

                    //If there are houses/hotel use the rent table 
                    if (houses > 0 && tile.RentByHouses != null && tile.RentByHouses.Length > houses)
                    {
                        int rent = tile.RentByHouses[houses];

                        return rent;
                    }

                    //check monopoly for doubled base rent
                    bool hasMonopoly =
                        ownership.CountOwnedInGroup(owner, tile.Group) >= rules.StreetsInGroup(tile.Group);

                    int finalRent = hasMonopoly ? tile.BaseRent * 2 : tile.BaseRent;

                    return finalRent;
                }

                 public static int RailroadRent(
                    Player owner,
                    IOwnershipService ownership,
                    IRuleSet rules)
                {
                    if (owner == null || ownership == null || rules == null)
                        return 0;

                    int count = Mathf.Clamp(ownership.CountRailroadsOwned(owner), 0, 4);
                    if (count <= 0)
                        return 0;

                    int baseRent = rules.RailroadBaseRent();
                    int rent = baseRent << (count - 1);

                    return rent;
                }

                public static int UtilityRent(
                    Player owner,
                    int diceTotal,
                    IOwnershipService ownership,
                    IRuleSet rules)
                {
                    if (owner == null || ownership == null || rules == null)
                        return 0;

                    int total = Mathf.Max(0, diceTotal);
                    bool ownsBoth = ownership.OwnsBothUtilities(owner);

                    int multiplier = ownsBoth
                        ? rules.UtilityRentBothMult()
                        : rules.UtilityRentSingleMult();

                    int rent = total * multiplier;

                    return rent;
                }
               
        }
}