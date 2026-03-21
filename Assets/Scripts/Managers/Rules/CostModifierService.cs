using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Managers.Rent;

namespace Assets.Scripts.Managers.Rent
{
    /// <summary>holds and uses rent modifiers (from cards/tile effects).</summary>
    public class CostModifierService : MonoBehaviour
    {
        private readonly List<IRentModifier> _mods = new ();

        // This should only be used for things that apply to EVERYTHING. So scalars for rules.
        private readonly List<IRentModifier> _permanent_mods = new ();
        public void Add(IRentModifier mod)
        {
            if (mod != null) _mods.Add(mod);
        }

        public void AddPermanent(IRentModifier mod)
        {
            if (mod != null) _permanent_mods.Add(mod);
        }

        //applies all active modifiers and removes expired (non-in use ones)
        public int ApplyAll(int baseRent, ITileRentInfo tile, Player tenant, Player owner)
        {
            int rent = baseRent;

            for (int i = _mods.Count - 1; i >= 0; i--)
            {
                var m = _mods[i];
                var pm = _permanent_mods[i];
                if (!m.IsActive())
                {
                    _mods.RemoveAt(i);
                    continue;
                }

                rent = Mathf.Max(0, pm.Apply(rent, tile, tenant, owner));
                rent = Mathf.Max(0, m.Apply(rent, tile, tenant, owner));
            }

            return rent;
        }

        //a lot of these are general use case helpers for future rule implementaiton
        //they're also intentionally built out kind of small for simplicities sake /scalability
        public void DoubleRentOnceForTile(ITileRentInfo targetTile)
            => Add(new DoubleOnceForTile(targetTile));

        public void FreeNextRentForTenant(Player tenant)
            => Add(new FreeOnceForTenant(tenant));


        private class DoubleOnceForTile : IRentModifier
        {
            private readonly ITileRentInfo _tile;
            private bool _used;
            public DoubleOnceForTile(ITileRentInfo tile) { _tile = tile; }
            public int Apply(int current, ITileRentInfo tile, Player tenant, Player owner)
            {
                if (_used || _tile == null || tile == null || !ReferenceEquals(tile, _tile)) return current;
                _used = true;
                return current * 2;
            }
            public bool IsActive() => !_used;
        }

        private class FreeOnceForTenant : IRentModifier
        {
            private readonly Player _tenant;
            private bool _used;
            public FreeOnceForTenant(Player t) { _tenant = t; }
            public int Apply(int current, ITileRentInfo tile, Player tenant, Player owner)
            {
                if (_used || tenant == null || _tenant == null || !ReferenceEquals(tenant, _tenant)) return current;
                _used = true;
                return 0;
            }
            public bool IsActive() => !_used;
        }
    }
}
