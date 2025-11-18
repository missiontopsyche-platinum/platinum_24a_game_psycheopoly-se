using NUnit.Framework;
using Events.EventDataStructures;
using System.Collections.Generic;
using UnityEngine;

// Refactor if we need more tests
public class PayAndCollectPerPropertyCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void PayPerProperty_ChargesCorrectTotal()
    {
        InitializePlayerManagerChannels();
        ChargePlayerEventChannel charge = playerManager.chargePlayerEventChannel;
        var raised = new List<ChargePlayerEvent>();
        charge.Subscribe(e => raised.Add(e));

        var effect = new PayPerPropertyCardEffect
        {
            ChargeForHouse = 20,
            ChargeForHotel = 100,
            chargePlayerEventChannel = charge
        };

        createPropertyForPlayerA();
        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(playerA, raised[0].chargedPlayer);
        Assert.AreEqual(140, raised[0].chargeAmount);
        Assert.AreEqual(1360, playerA.GetMoney());
    }

    [Test]
    public void PayPerProperty_NoOwnedProperties()
    {
        InitializePlayerManagerChannels();
        ChargePlayerEventChannel charge = playerManager.chargePlayerEventChannel;
        var raised = new List<ChargePlayerEvent>();
        charge.Subscribe(e => raised.Add(e));

        var effect = new PayPerPropertyCardEffect
        {
            ChargeForHouse = 20,
            ChargeForHotel = 100,
            chargePlayerEventChannel = charge
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(0, raised.Count);
        Assert.AreEqual(1500, playerA.GetMoney());
    }

    [Test]
    public void PayPerProperty_NoCharge()
    {
        InitializePlayerManagerChannels();
        ChargePlayerEventChannel charge = playerManager.chargePlayerEventChannel;
        var raised = new List<ChargePlayerEvent>();
        charge.Subscribe(e => raised.Add(e));

        var effect = new PayPerPropertyCardEffect
        {
            ChargeForHouse = 0,
            ChargeForHotel = 0,
            chargePlayerEventChannel = charge
        };

        createPropertyForPlayerA();
        effect.ApplyEffect(playerA);

        Assert.AreEqual(0, raised.Count);
        Assert.AreEqual(1500, playerA.GetMoney());
    }

    [Test]
    public void CollectPerProperty_PaysCorrectTotal()
    {
        InitializePlayerManagerChannels();
        PayPlayerEventChannel pay = playerManager.payPlayerEventChannel;
        var raised = new List<PayPlayerEvent>();
        pay.Subscribe(e => raised.Add(e));

        var effect = new CollectPerPropertyCardEffect
        {
            ChargeForHouse = 20,
            ChargeForHotel = 100,
            payPlayerEventChannel = pay
        };

        createPropertyForPlayerA();
        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(playerA, raised[0].paidPlayer);
        Assert.AreEqual(140, raised[0].amountPaid);
        Assert.AreEqual(1640, playerA.GetMoney());
    }

    [Test]
    public void CollectPerProperty_NoOwnedProperties()
    {
        InitializePlayerManagerChannels();
        PayPlayerEventChannel pay = playerManager.payPlayerEventChannel;
        var raised = new List<PayPlayerEvent>();
        pay.Subscribe(e => raised.Add(e));

        var effect = new CollectPerPropertyCardEffect
        {
            ChargeForHouse = 20,
            ChargeForHotel = 100,
            payPlayerEventChannel = pay
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(0, raised.Count);
        Assert.AreEqual(1500, playerA.GetMoney());
    }

    [Test]
    public void CollectPerProperty_NoCharge()
    {
        InitializePlayerManagerChannels();
        PayPlayerEventChannel pay = playerManager.payPlayerEventChannel;
        var raised = new List<PayPlayerEvent>();
        pay.Subscribe(e => raised.Add(e));

        var effect = new CollectPerPropertyCardEffect
        {
            ChargeForHouse = 0,
            ChargeForHotel = 0,
            payPlayerEventChannel = pay
        };

        createPropertyForPlayerA();
        effect.ApplyEffect(playerA);

        Assert.AreEqual(0, raised.Count);
        Assert.AreEqual(1500, playerA.GetMoney());
    }

    private void createPropertyForPlayerA()
    {
        PropertySpaceData property1 = ScriptableObject.CreateInstance<PropertySpaceData>();
        property1.buyPrice = 100;
        property1.SetOwner(playerA);
        playerA.AddOwnedProperty(property1);
        for (int i = 0; i < 5; i++) property1.UpgradeProperty();

        PropertySpaceData property2 = ScriptableObject.CreateInstance<PropertySpaceData>();
        property2.buyPrice = 100;
        property2.SetOwner(playerA);
        playerA.AddOwnedProperty(property2);
        property2.UpgradeProperty();

        PropertySpaceData property3 = ScriptableObject.CreateInstance<PropertySpaceData>();
        property3.buyPrice = 100;
        property3.SetOwner(playerA);
        playerA.AddOwnedProperty(property3);
        property3.UpgradeProperty();
    }
}
