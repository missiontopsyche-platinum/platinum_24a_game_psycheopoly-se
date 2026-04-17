using Assets.Scripts.Managers.Rent;

namespace Assets.Scripts.Managers.Rent
{
    /// <summary>Rent modification contract applied after base rent is computed.</summary>
    /// this basically will act as the rent modification ruleset
    //US402-T406
    public interface IRentModifier
    {
        /// <summary>return modified rent given the current context.</summary>
        int Apply(int currentRent, ITileRentInfo tile, Player tenant, Player owner);

        /// <summary>if this modifier still applies; if false it will be removed.</summary>
        bool IsActive();
    }
}
