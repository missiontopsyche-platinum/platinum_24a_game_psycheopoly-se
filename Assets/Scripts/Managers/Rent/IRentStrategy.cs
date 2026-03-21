using Assets.Scripts.Managers.Rules;


namespace Assets.Scripts.Managers.Rent
{
    public interface IRentStrategy
    {
        /// <summary>Returns rent due for landing.</summary>
        int ComputeRent(ITileRentInfo tile, Player owner, int diceTotal, IOwnershipService own, StandardRuleSet rules);
    }
}
