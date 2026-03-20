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

                        Logger.Debug(
                            "RentCalculator.Street",
                            $"[{tile.Name}] HouseCount={houses} Rent={rent}",
                            LogCategory.Gameplay);

                        return rent;
                    }

                    //check monopoly for doubled base rent
                    bool hasMonopoly =
                        ownership.CountOwnedInGroup(owner, tile.Group) >= rules.StreetsInGroup(tile.Group);

                    int finalRent = hasMonopoly ? tile.BaseRent * 2 : tile.BaseRent;

                    Logger.Debug(
                        "RentCalculator.Street",
                        $"[{tile.Name}] Houses=0 BaseRent={tile.BaseRent} Monopoly={hasMonopoly} Final={finalRent}",
                        LogCategory.Gameplay);

                    return finalRent;
                }

               
        }
}