using System;
using Data;
using UnityEngine;


public static class UpgradeUtility
{
    
    public static UpgradeDecision Evaluate(Player owner, IUpgradableTileInfo tile, IUpgradableTileInfo[] monoployGroup)
    {
        if (owner == null || tile == null)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.InvalidRequest);
        }

        if (tile.GetOwner() != owner)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.NotOwner);
        }

        if (tile is not PropertySpaceData)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.NotStreet);
        }

        if (tile.IsMortgaged)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.Mortgaged);
        }

        if (!OwnsFullMonopoly(owner, monoployGroup))
        {
            return UpgradeDecision.Failed(UpgradeFailReason.MonopolyNotOwned);
        }

        if (!BuildsEvenly(tile, monoployGroup))
        {
            return UpgradeDecision.Failed(UpgradeFailReason.UnevenBuilding);
        }


        if (tile.IsMaxed)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.AlreadyMaxed);
        }

        int cost = Mathf.Max(0, tile.GetNextUpgradeCost());
        if (cost <= 0)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.InvalidUpgradeCost);
        }

        if (owner.GetMoney() < cost)
        {
            return UpgradeDecision.Failed(UpgradeFailReason.InsufficientFunds, cost);
        }

        return UpgradeDecision.Success(cost);
    }

    public static bool TryExecute(Player owner, IUpgradableTileInfo tile, UpgradeDecision decision)
    {
        if (!decision.Allowed || owner == null || tile == null)
        {
            return false;
        }

        if (owner.TrySpend(decision.Cost) != Player.FinancialStatus.Success)
        {
            return false;
        }

        tile.ApplyUpgrade();
        return true;

        return false;
    }

    private static bool OwnsFullMonopoly(Player owner, IUpgradableTileInfo[] monopolyGroup)
    {
        if (owner == null || monopolyGroup == null || monopolyGroup.Length == 0)
            return false;

        foreach (var property in monopolyGroup)
        {
            if (property == null)
                return false;

            if (property.GetOwner() != owner)
                return false;
        }

        return true;
    }

    private static bool BuildsEvenly(IUpgradableTileInfo tile, IUpgradableTileInfo[] monopolyGroup)
    {
        if (tile == null || monopolyGroup == null || monopolyGroup.Length == 0)
            return false;

        int minLevel = int.MaxValue;

        foreach (var property in monopolyGroup)
        {
            if (property == null)
                return false;

            if (property.UpgradeLevel < minLevel)
            {
                minLevel = property.UpgradeLevel;
            }
        }

        return tile.UpgradeLevel == minLevel;
    }
}