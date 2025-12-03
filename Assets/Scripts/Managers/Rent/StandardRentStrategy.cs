using System;
using UnityEngine;
using Logging;
using Assets.Scripts.Managers.Rules;

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
            //Completes task 283
            //edited for task403
            if (houses > 0 && tile.RentByHouses != null && tile.RentByHouses.Length > houses)
            {
                int rent = tile.RentByHouses[houses];

                Logging.Logger.Debug("RentStrategy.Street",
                    $"[{tile.Name}] HouseCount={houses} Rent={rent}",
                    LogCategory.Gameplay);

                return rent;
            }

            //If there are no houses check monoploy which doubles base rent
            bool hasMonopoly =
               own.CountOwnedInGroup(owner, tile.Group) >= rules.StreetsInGroup(tile.Group);

            int finalRent = hasMonopoly ? tile.BaseRent * 2 : tile.BaseRent;

            Logging.Logger.Debug("RentStrategy.Street",
                $"[{tile.Name}] Houses=0 BaseRent={tile.BaseRent} Monopoly={hasMonopoly} Final={finalRent}",
                LogCategory.Gameplay);

            return finalRent;


        }

        private int RailroadRent(Player owner, IOwnershipService own, IRuleSet rules)
        {
            int count = Mathf.Clamp(own.CountRailroadsOwned(owner), 0, 4);
            if (count <= 0) return 0;

            int baseRent = rules.RailroadBaseRent();

            int rent = baseRent << (count - 1);

            Logging.Logger.Debug("RentStrategy.Railroad",
                $"RailroadsOwned={count} BaseRent={baseRent} Final={rent}",
                LogCategory.Gameplay);

            return rent;
        }

        private int UtilityRent(Player owner, int diceTotal, IOwnershipService own, IRuleSet rules)
        {
            int total = Mathf.Max(0, diceTotal);
            bool ownsBoth = own.OwnsBothUtilities(owner);


            int mult = ownsBoth ?
                rules.UtilityRentBothMult() :
                rules.UtilityRentSingleMult();

            int rent = total * mult;

            Logging.Logger.Debug("RentStrategy.Utility",
                $"Dice={total} OwnsBoth={ownsBoth} Mult={mult} Final={rent}",
                LogCategory.Gameplay);

            return rent;


        }

    }




}