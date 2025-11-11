using System;
using Events.EventDataStructures;
using Logging;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanetSpaceData", menuName = "Board Spaces/Planet Space")]
public class PlanetSpaceData : OwnableSpaceData
{
    [SerializeField] public int[] diceMultipliers = new int[2];
    [SerializeField] public DiceRolledEventChannel diceRolledEventChannel;

    public int lastDiceRoll = 0;
    
    public override void OnLanded(Player player)
    {
        base.OnLanded(player);
        if (owner == null) return;

        if (owner.Equals(player))
        {
            // do nothing based on rules
        }
        else
        {
            // check how many planets the player owns (1 or 2)
            // and apply the multiplier at the (num-1) index of the
            // dice multipliers array, and send the charge event.
            int numPlanetsOwned = owner.GetNumberPlanetsOwned();
            switch (numPlanetsOwned)
            {
                case 1 or 2:
                    int multiplierAmount = diceMultipliers[numPlanetsOwned - 1];
                    int charge = multiplierAmount * lastDiceRoll;
                    chargeOwnershipFeeEventChannel?.RaiseEvent(new ChargeOwnershipFeeEvent(
                        player, owner, charge, this));
                    break;
                case 0:
                    Logging.Logger.Error("PlanetSpaceData.OnLanded",
                        "Owner does not have record of owning any Planets",
                        LogCategory.Economy,
                        this);
                    break;
                default:
                    Logging.Logger.Error("PlanetSpaceData.OnLanded",
                        $"Owner owns an invalid number of planets: {numPlanetsOwned}",
                        LogCategory.Economy,
                        this);
                    break;
            }
        }
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        SpaceHoverEvent payload = base.OnHover();
        
        // just copying directly from the source spaces.
        payload.AppendInformation(
            "If one planet is being studied, research funding is " +
            diceMultipliers[0] +
            " times the amount shown on the dice.");
        payload.AppendInformation(
            "If both planets are being studied, research funding is " +
            diceMultipliers[0] +
            " times the amount shown on the dice.");

        spaceHoverEventChannel?.RaiseEvent(payload);
        return payload;
    }

    /// <summary>
    /// Takes in the DiceRolledEvent and stores the last value of the dice roll locally
    /// for resolving multipliers.
    /// </summary>
    /// <param name="dre">Event payload for a dice roll.</param>
    private void StoreLastDiceRoll(DiceRolledEvent dre)
    {
        lastDiceRoll = dre.totalRoll;
    }

    public void EnsureSubscribed()
    {
        OnDisable();
        OnEnable();
    }

    public void OnEnable() => diceRolledEventChannel?.Subscribe(StoreLastDiceRoll);
    public void OnDisable() => diceRolledEventChannel?.Unsubscribe(StoreLastDiceRoll);
}
