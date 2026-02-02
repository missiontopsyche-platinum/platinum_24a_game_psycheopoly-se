using UnityEngine;
using Assets.Scripts.Managers.Rent;

namespace Assets.Scripts.Managers.Rules
{
    /// <summary>
    /// basic Monopoly rule implementation still an empty placeholder for US402-T404
    /// Will be populated when rent rule logic is brought oever
    /// </summary>
    [System.Serializable]
    public class StandardRuleSet : IRuleSet
    {
        [Tooltip("Base rent for 1 owned railroad (25, 50, 100, 200 scaling).")]
        [SerializeField] private int railroadBase = 25;

        [Tooltip("Utility multiplier when owner has a single utility.")]
        [SerializeField] private int utilitySingle = 4;

        [Tooltip("Utility multiplier when owner has both utilities.")]
        [SerializeField] private int utilityBoth = 10;

        [Tooltip("Starting money for each player.")]
        [SerializeField] private int playerStartingMoney = 1500;

        [Tooltip("Salary received when passing GO.")]
        [SerializeField] private int goSalary = 200;

        [Tooltip("Fee to pay when leaving jail.")]
        [SerializeField] private int jailFee = 100;

        [Tooltip("Win condition type for the game.")]
        [SerializeField] private WinConditionType winCondition = WinConditionType.LastPlayerStanding;

        [Tooltip("Target money for TargetMoney win condition.")]
        [SerializeField] private int targetMoney = 5000;

        [Tooltip("Turn limit for TurnLimit win condition.")]
        [SerializeField] private int turnLimit = 20;

        [Tooltip("Max turn limit for jail time.")]
        [SerializeField] private int maxJailTurns = 3;

        public int RailroadBaseRent() => railroadBase;
        public int UtilityRentSingleMult() => utilitySingle;
        public int UtilityRentBothMult() => utilityBoth;
        public int StreetsInGroup(ColorGroup g) =>
            g switch
            {
                ColorGroup.Brown or ColorGroup.DarkBlue => 2,
                ColorGroup.LightBlue or ColorGroup.Pink or ColorGroup.Orange or ColorGroup.Red or ColorGroup.Yellow or ColorGroup.Green => 3,
                _ => 0
            };
        public int PlayerStartingMoney() => playerStartingMoney;
        public int GOSalary() => goSalary;
        public int JailFee() => jailFee;
        public WinConditionType WinCondition() => winCondition;
        public int TargetMoney() => targetMoney;
        public int TurnLimit() => turnLimit;

        public int MaxJailTurns() => maxJailTurns;
    }
}