using NUnit.Framework;
using System.Collections.Generic;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;
using UnityEngine;

public class JailCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void GoToJailCardEffect_RaisesJailStateChanged_WithTurns()
    {
        JailStateChangedEventChannel jailChannel = CreateChannel<JailStateChangedEventChannel>();
        List<JailStateChangedEvent> raised = new();
        jailChannel.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<GoToJailCardEffect>());
        effect.TurnsInJail = 2;
        effect.JailStateChangedEventChannel = jailChannel;
        
        effect.ApplyEffect(testPlayer);
        
        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(testPlayer, raised[0].player);
        Assert.IsTrue(raised[0].inJail);
        Assert.AreEqual(2, raised[0].jailTurns);
    }

    [Test]
    public void GetOutOfJailCardEffect_RaisesReleaseEvent()
    {
        JailStateChangedEventChannel jailChannel = CreateChannel<JailStateChangedEventChannel>();
        List<JailStateChangedEvent> raised = new();
        jailChannel.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<GetOutOfJailCardEffect>());
        effect.JailStateChangedEventChannel = jailChannel;

        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(testPlayer, raised[0].player);
        Assert.IsFalse(raised[0].inJail);
        Assert.AreEqual(0, raised[0].jailTurns);
    }
}
