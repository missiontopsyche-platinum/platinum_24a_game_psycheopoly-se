using System.Collections.Generic;
using System.Linq;
using Data;
using Logging;
using UnityEngine;

namespace AIBehavior
{
    public class AIUnmortgageBehavior
    {
        private readonly Player player;
        private readonly MortgageThresholds thresholds;

        public AIUnmortgageBehavior(Player player, MortgageThresholds thresholds)
        {
            this.player = player;
            this.thresholds = thresholds;
        }

        public AIUnmortgageEvaluation EvaluateUnmortgage()
        {
            List<OwnableSpaceData> mortgagedProperties = player.GetMortgagedProperties();

            if (mortgagedProperties == null || mortgagedProperties.Count == 0)
            {
                return AIUnmortgageEvaluation.Defer("No mortgaged properties to evaluate.");
            }

            int reserve = thresholds.unmortgageReserveThreshold;
            int availableCash = player.GetMoney() - reserve;

            if (availableCash <= 0)
            {
                return AIUnmortgageEvaluation.Defer(
                    $"Insufficient surplus cash. Money={player.GetMoney()}, Reserve={reserve}.");
            }

            List<UnmortgageCandidate> candidates = mortgagedProperties
                .Select(space => new UnmortgageCandidate(
                    space,
                    GetUnmortgageCost(space),
                    GetIncomePotential(space)))
                .Where(candidate => candidate.unmortgageCost > 0)
                .OrderByDescending(candidate => candidate.incomePotential)
                .ThenBy(candidate => candidate.unmortgageCost)
                .ToList();

            Queue<MortgageAction> actions = new();

            foreach (UnmortgageCandidate candidate in candidates)
            {
                if (candidate.unmortgageCost > availableCash)
                    continue;

                actions.Enqueue(new MortgageAction(
                    candidate.space,
                    MortgageActionType.Unmortgage,
                    -candidate.unmortgageCost));

                availableCash -= candidate.unmortgageCost;
            }

            if (actions.Count == 0)
            {
                return AIUnmortgageEvaluation.Defer("No affordable unmortgage actions were found.");
            }

            Logging.Logger.Info("AIUnmortgageBehavior.EvaluateUnmortgage",
                $"AI {player.GetPName()} selected {actions.Count} unmortgage action(s).",
                LogCategory.AI);

            return AIUnmortgageEvaluation.Found(actions);
        }

        private int GetUnmortgageCost(OwnableSpaceData ownable)
        {
            if (ownable.mortgagePayoffValue > 0)
                return ownable.mortgagePayoffValue;

            return Mathf.RoundToInt(ownable.collaborationValue * 1.10f);
        }

        private int GetIncomePotential(OwnableSpaceData ownable)
        {
            return ownable switch
            {
                PropertySpaceData property =>
                    property.researchFundingValues != null &&
                    property.researchFundingValues.Length > property.GetCurrentUpgradeLevel()
                        ? property.researchFundingValues[property.GetCurrentUpgradeLevel()]
                        : 0,

                InstrumentSpaceData instrument =>
                    instrument.researchFundingLevels != null &&
                    instrument.researchFundingLevels.Length > 0
                        ? instrument.researchFundingLevels[
                            Mathf.Clamp(player.GetNumberInstrumentsOwned() - 1, 0,
                                instrument.researchFundingLevels.Length - 1)]
                        : 0,

                PlanetSpaceData planet =>
                    planet.diceMultipliers != null &&
                    planet.diceMultipliers.Length > 0
                        ? planet.diceMultipliers[
                            Mathf.Clamp(player.GetNumberPlanetsOwned() - 1, 0,
                                planet.diceMultipliers.Length - 1)] * 7
                        : 0,

                _ => 0
            };
        }

        private class UnmortgageCandidate
        {
            public OwnableSpaceData space { get; }
            public int unmortgageCost { get; }
            public int incomePotential { get; }

            public UnmortgageCandidate(OwnableSpaceData space, int unmortgageCost, int incomePotential)
            {
                this.space = space;
                this.unmortgageCost = unmortgageCost;
                this.incomePotential = incomePotential;
            }
        }
    }

    public class AIUnmortgageEvaluation
    {
        public bool willUnmortgage { get; }
        public Queue<MortgageAction> actions { get; }
        public string message { get; }

        private AIUnmortgageEvaluation(bool willUnmortgage, Queue<MortgageAction> actions, string message)
        {
            this.willUnmortgage = willUnmortgage;
            this.actions = actions;
            this.message = message;
        }

        public static AIUnmortgageEvaluation Found(Queue<MortgageAction> actions)
        {
            return new AIUnmortgageEvaluation(true, actions, "Unmortgage actions selected.");
        }

        public static AIUnmortgageEvaluation Defer(string message)
        {
            return new AIUnmortgageEvaluation(false, new Queue<MortgageAction>(), message);
        }
    }
}