using Data;
using NUnit.Framework;
using UnityEngine;

public class MortageEditModeTests : MortgageTestBase
{
    [SetUp]
    public void SetUp()
    {   
        
    }

    [Test]
    public void MortgageAction_ReturnsTrue()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .BuildAsProperty());

        property.isMortgageable = true;
        property.isMortgaged = false;

        var player = Track(APlayer()
            .WithMoney(100)
            .WithOwnedProperty(property)
            .Build());

        Assert.IsTrue(player.MortgageProperty(property));
    }

    [Test]
    public void MortgageAction_AddsMoney()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .BuildAsProperty());

        property.isMortgageable = true;
        property.isMortgaged = false;

        var player = Track(APlayer()
            .WithMoney(100)
            .WithOwnedProperty(property)
            .Build());

        player.MortgageProperty(property);

        Assert.AreEqual(150, player.GetMoney());
    }

    [Test]
    public void MortgageAction_AllPropertyTypesAreMortgageable()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .BuildAsProperty());

        property.isMortgageable = true;
        property.isMortgaged = false;

        var instrument = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .BuildAsInstrument());

        instrument.isMortgageable = true;
        instrument.isMortgaged = false;

        var planet = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .BuildAsPlanet());

        planet.isMortgageable = true;
        planet.isMortgaged = false;

        var player = Track(APlayer()
            .WithMoney(100)
            .WithOwnedProperty(property)
            .WithOwnedProperty(instrument)
            .WithOwnedProperty(planet)
            .Build());

        Assert.IsTrue(player.MortgageProperty(property));
        Assert.IsTrue(player.MortgageProperty(instrument));
        Assert.IsTrue(player.MortgageProperty(planet));
    }

    [Test]
    public void MortgageAction_DoNotMortageIfFlagIsFalse()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .BuildAsProperty());

        property.isMortgageable = false;
        property.isMortgaged = false;

        var player = Track(APlayer()
            .WithMoney(100)
            .WithOwnedProperty(property)
            .Build());

        Assert.IsFalse(player.MortgageProperty(property));
    }

    [Test]
    public void MortgageAction_PayoffMortgageSuccess()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .WithMortgagePayoffValue(55)
            .BuildAsProperty());

        property.isMortgageable = false;
        property.isMortgaged = true;

        var player = Track(APlayer()
            .WithMoney(100)
            .WithOwnedProperty(property)
            .Build());

        Assert.IsTrue(player.UnmortgageProperty(property));
    }

    [Test]
    public void MortgageAction_MortgagePayoffSetupCorrectly()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .WithMortgagePayoffValue(1)
            .BuildAsProperty());

        property.isMortgageable = true;
        property.isMortgaged = false;

        var player = Track(APlayer()
            .WithMoney(100)
            .WithOwnedProperty(property)
            .Build());

        player.MortgageProperty(property);

        Assert.AreEqual(55, property.mortgagePayoffValue);
    }

    [Test]
    public void MortgageAction_StopsUpgrades()
    {
        var property = Track(AnOwnableSpace()
            .WithCollabValue(50)
            .WithMortgagePayoffValue(1)
            .BuildAsProperty());

        property.isMortgageable = false;
        property.isMortgaged = true;

        Assert.IsFalse(property.CanUpgrade());
    }
}
