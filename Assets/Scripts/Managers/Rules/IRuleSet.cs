using UnityEngine;
using Assets.Scripts.Managers.Rent;


namespace Assets.Scripts.Managers.Rules
{
    /// <summary>
    /// interface for rule sets used by rent strategy 
    /// and later tile effects
    /// **placeholder for architecture setup in US402-T404**
    /// </summary>
    public interface IRuleSet
    {
        int RailroadBaseRent();
        int UtilityRentSingleMult();
        int UtilityRentBothMult();
        int StreetsInGroup(ColorGroup group);
        int PlayerStartingMoney();
        int GOSalary();
        int JailFee();
        WinConditionType WinCondition();
        int TargetMoney();
        int TurnLimit();
        int MaxJailTurns();
    }
    public enum WinConditionType
    {
        LastPlayerStanding,
        TargetMoney,
        TurnLimit
    }
}

