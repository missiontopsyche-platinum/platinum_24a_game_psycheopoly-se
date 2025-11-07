using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers.Rent
{
    //Replace later with real ownership source
    public class OwnershipServiceAdapter : MonoBehaviour, IOwnershipService
    {
        private readonly Dictionary<ITileRentInfo, Player> owners = new();
        private readonly Dictionary<Player, HashSet<ITileRentInfo>> ownedByPlayer = new();

        //public API
        public void SetOwner(ITileRentInfo tile, Player owner)
        {
            owners[tile] = owner;

            if (!ownedByPlayer.TryGetValue(owner, out var set))
            {
                set = new HashSet<ITileRentInfo>();
                ownedByPlayer[owner] = set;
            }
            set.Add(tile);
        }

        //IOwnershipService
        public Player GetOwner(ITileRentInfo tile)
        {
            owners.TryGetValue(tile, out var p);
            return p;
        }

        public int CountOwnedInGroup(Player owner, ColorGroup group)
        {
            if (owner == null) return 0;
            if (!ownedByPlayer.TryGetValue(owner, out var set)) return 0;

            int c = 0;
            foreach (var t in set)
                if (t.Type == TileType.Street && t.Group == group) c++;
            return c;
        }

        public int CountRailroadsOwned(Player owner)
        {
            if (owner == null) return 0;
            if (!ownedByPlayer.TryGetValue(owner, out var set)) return 0;

            int c = 0;
            foreach (var t in set)
                if (t.Type == TileType.Railroad) c++;
            return c;
        }

        public bool OwnsBothUtilities(Player owner)
        {
            return CountUtilities(owner) >= 2;
        }

        //helper
        private int CountUtilities(Player owner)
        {
            if (owner == null) return 0;
            if (!ownedByPlayer.TryGetValue(owner, out var set)) return 0;

            int c = 0;
            foreach (var t in set)
                if (t.Type == TileType.Utility) c++;
            return c;
        }
    }
}
