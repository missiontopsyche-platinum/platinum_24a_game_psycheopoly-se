using NUnit.Framework;
using Events.EventDataStructures;
using System.Collections.Generic;
using UnityEngine;

// Refactor if we need more tests
public class PayAndCollectPerPropertyCardEffectTest : CardEffectBaseTest
{
    private void CreatePropertyForTestPlayer()
    {
        PropertySpaceData property1 = TrackScriptableObject(ScriptableObject.CreateInstance<PropertySpaceData>());
        property1.buyPrice = 100;
        property1.SetOwner(testPlayer);
        testPlayer.AddOwnedProperty(property1);
        for (int i = 0; i < 5; i++) property1.UpgradeProperty();

        PropertySpaceData property2 = TrackScriptableObject(ScriptableObject.CreateInstance<PropertySpaceData>());
        property2.buyPrice = 100;
        property2.SetOwner(testPlayer);
        testPlayer.AddOwnedProperty(property2);
        property2.UpgradeProperty();

        PropertySpaceData property3 = TrackScriptableObject(ScriptableObject.CreateInstance<PropertySpaceData>());
        property3.buyPrice = 100;
        property3.SetOwner(testPlayer);
        testPlayer.AddOwnedProperty(property3);
        property3.UpgradeProperty();
    }

    [Test]
    public void PayPerProperty_ChargesCorrectTotal()
    {
        ChargePlayerEventChannel charge = CreateChannel<ChargePlayerEventChannel>();
        List<ChargePlayerEvent> raised = new();
        charge.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<PayPerPropertyCardEffect>());
        effect.ChargeForHouse = 20;
        effect.ChargeForHotel = 100;
        effect.chargePlayerEventChannel = charge;

        CreatePropertyForTestPlayer();
        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(testPlayer, raised[0].chargedPlayer);
        Assert.AreEqual(140, raised[0].chargeAmount);
    }

    [Test]
    public void PayPerProperty_NoOwnedProperties()
    {
        ChargePlayerEventChannel charge = CreateChannel<ChargePlayerEventChannel>();
        List<ChargePlayerEvent> raised = new();
        charge.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<PayPerPropertyCardEffect>());
        effect.ChargeForHouse = 20;
        effect.ChargeForHotel = 100;
        effect.chargePlayerEventChannel = charge;

        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(0, raised.Count);
    }

    [Test]
    public void PayPerProperty_NoCharge()
    {
        ChargePlayerEventChannel charge = CreateChannel<ChargePlayerEventChannel>();
        List<ChargePlayerEvent> raised = new();
        charge.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<PayPerPropertyCardEffect>());
        effect.ChargeForHouse = 0;
        effect.ChargeForHotel = 0;
        effect.chargePlayerEventChannel = charge;

        CreatePropertyForTestPlayer();
        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(0, raised.Count);
    }

    [Test]
    public void CollectPerProperty_PaysCorrectTotal()
    {
        PayPlayerEventChannel pay = CreateChannel<PayPlayerEventChannel>();
        List<PayPlayerEvent> raised = new();
        pay.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<CollectPerPropertyCardEffect>());
        effect.ChargeForHouse = 20;
        effect.ChargeForHotel = 100;
        effect.payPlayerEventChannel = pay;

        CreatePropertyForTestPlayer();
        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(testPlayer, raised[0].paidPlayer);
        Assert.AreEqual(140, raised[0].amountPaid);
    }

    [Test]
    public void CollectPerProperty_NoOwnedProperties()
    {
        PayPlayerEventChannel pay = CreateChannel<PayPlayerEventChannel>();
        List<PayPlayerEvent> raised = new();
        pay.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<CollectPerPropertyCardEffect>());
        effect.ChargeForHouse = 20;
        effect.ChargeForHotel = 100;
        effect.payPlayerEventChannel = pay;

        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(0, raised.Count);
    }

    [Test]
    public void CollectPerProperty_NoCharge()
    {
        PayPlayerEventChannel pay = CreateChannel<PayPlayerEventChannel>();
        List<PayPlayerEvent> raised = new();
        pay.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<CollectPerPropertyCardEffect>());
        effect.ChargeForHouse = 0;
        effect.ChargeForHotel = 0;
        effect.payPlayerEventChannel = pay;

        CreatePropertyForTestPlayer();
        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(0, raised.Count);
    }
}
