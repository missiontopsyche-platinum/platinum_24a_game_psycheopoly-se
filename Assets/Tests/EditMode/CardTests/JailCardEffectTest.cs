using NUnit.Framework;
using System.Collections.Generic;
using Assets.Scripts.Events.EventChannelTypes;
using Assets.Scripts.Events.EventDataStructures;

public class JailCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void GoToJailCardEffect_RaisesJailStateChanged_WithTurns()
    {
        InitializePlayerManagerChannels();
        JailStateChangedEventChannel jailChannel = playerManager.jailStateChangedEventChannel;
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
        Assert.True(playerA.GetInJail());
        Assert.AreEqual(2, playerA.GetJailTurns());
    }

    [Test]
    public void GetOutOfJailCardEffect_RaisesReleaseEvent()
    {
        InitializePlayerManagerChannels();
        JailStateChangedEventChannel jailChannel = playerManager.jailStateChangedEventChannel;
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
        Assert.False(playerA.GetInJail());
        Assert.AreEqual(0, playerA.GetJailTurns());
    }
}
