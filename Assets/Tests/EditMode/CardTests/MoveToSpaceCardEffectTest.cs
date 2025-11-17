using NUnit.Framework;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class MoveToSpaceCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void ApplyEffect_RaiseMoveToSpaceEvent()
    {
        InitializeBoardManagerChannels();

        var effect = ScriptableObject.CreateInstance<MoveToSpaceCardEffect>();
        var channel = boardManager.moveToSpaceEventChannel;

        bool eventRaised = false;
        MoveToSpaceEvent capturedEvent = default;

        channel.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        effect.moveToSpaceEventChannel = channel;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.PropertySpace;

        effect.ApplyEffect(playerA);

        Assert.IsTrue(eventRaised);
        Assert.AreSame(playerA, capturedEvent.player);
        Assert.AreEqual(effect.targetType, capturedEvent.targetKind);
    }

    [Test]
    public void ApplyEffect_DoesNotRaiseEvent_WhenPlayerIsNull()
    {
        InitializeBoardManagerChannels();

        var effect = ScriptableObject.CreateInstance<MoveToSpaceCardEffect>();
        var channel = boardManager.moveToSpaceEventChannel;

        bool eventRaised = false;

        channel.Subscribe(_ => { eventRaised = true; });

        effect.moveToSpaceEventChannel = channel;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.GoSpace;

        effect.ApplyEffect(null);

        Assert.IsFalse(eventRaised);
    }

    [Test]
    public void ApplyEffect_DoesNotThrowAndDoesNotRaiseEvent_WhenChannelIsNull()
    {
        var effect = ScriptableObject.CreateInstance<MoveToSpaceCardEffect>();
        effect.moveToSpaceEventChannel = null;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.CardSpace;

        Assert.DoesNotThrow(() => effect.ApplyEffect(playerA),
            "ApplyEffect should safely no-op when the channel is null.");
    }

    [Test]
    public void ApplyEffect_CanBeCalledMultipleTimes_RaisesEventEachTime()
    {
        var effect = ScriptableObject.CreateInstance<MoveToSpaceCardEffect>();
        var channel = boardManager.moveToSpaceEventChannel;

        int callCount = 0;

        channel.Subscribe(_ => { callCount++; });

        effect.moveToSpaceEventChannel = channel;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace;

        effect.ApplyEffect(playerA);
        effect.ApplyEffect(playerB);
        effect.ApplyEffect(playerC);

        Assert.AreEqual(3, callCount);
    }
}
