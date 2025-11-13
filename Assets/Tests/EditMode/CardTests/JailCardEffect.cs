using NUnit.Framework;
using System.Collections.Generic;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;

public class JailCardEffect : CardEffectBaseTest
{
    [Test]
    public void GoToJailCardEffect_RaisesJailStateChanged_WithTurns()
    {
        JailStateChangedEventChannel jailChannel = CreateChannel<JailStateChangedEventChannel>();
        List<JailStateChangedEvent> raised = new List<JailStateChangedEvent>();
        jailChannel.Subscribe(e => raised.Add(e));

        GoToJailCardEffect effect = new GoToJailCardEffect
        {
            TurnsInJail = 2,
            JailStateChangedEventChannel = jailChannel
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(playerA, raised[0].player);
        Assert.AreEqual(2, raised[0].jailTurns);
    }

    [Test]
    public void GetOutOfJailCardEffect_RaisesReleaseEvent()
    {
        JailStateChangedEventChannel jailChannel = CreateChannel<JailStateChangedEventChannel>();
        List<JailStateChangedEvent> raised = new List<JailStateChangedEvent>();
        jailChannel.Subscribe(e => raised.Add(e));

        GetOutOfJailCardEffect effect = new GetOutOfJailCardEffect
        {
            JailStateChangedEventChannel = jailChannel
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, raised.Count);
        Assert.AreSame(playerA, raised[0].player);
        Assert.AreEqual(0, raised[0].jailTurns);
    }
}
