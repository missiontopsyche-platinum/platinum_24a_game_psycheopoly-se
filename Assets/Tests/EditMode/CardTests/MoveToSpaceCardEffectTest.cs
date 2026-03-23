using NUnit.Framework;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Tests.EditMode.CardTests
{
    public class MoveToSpaceCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void ApplyEffect_RaiseMoveToSpaceEvent()
    {
        MoveToSpaceEventChannel channel = CreateChannel<MoveToSpaceEventChannel>();
        bool eventRaised = false;
        MoveToSpaceEvent capturedEvent = default;
        channel.Subscribe(evt =>
        {
            eventRaised = true;
            capturedEvent = evt;
        });

        var effect = TrackEffect(ScriptableObject.CreateInstance<MoveToSpaceCardEffect>());
        effect.moveToSpaceEventChannel = channel;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.PropertySpace;

        effect.ApplyEffect(testPlayer);

        Assert.IsTrue(eventRaised);
        Assert.AreSame(testPlayer, capturedEvent.player);
        Assert.AreEqual(effect.targetType, capturedEvent.targetKind);
    }

    [Test]
    public void ApplyEffect_DoesNotRaiseEvent_WhenPlayerIsNull()
    {
        MoveToSpaceEventChannel channel = CreateChannel<MoveToSpaceEventChannel>();
        bool eventRaised = false;
        channel.Subscribe(_ => { eventRaised = true; });

        var effect = TrackEffect(ScriptableObject.CreateInstance<MoveToSpaceCardEffect>());
        effect.moveToSpaceEventChannel = channel;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.GoSpace;

        effect.ApplyEffect(null);

        Assert.IsFalse(eventRaised);
    }

    [Test]
    public void ApplyEffect_DoesNotThrowAndDoesNotRaiseEvent_WhenChannelIsNull()
    {
        var effect = TrackEffect(ScriptableObject.CreateInstance<MoveToSpaceCardEffect>());
        effect.moveToSpaceEventChannel = null;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.CardSpace;

        Assert.DoesNotThrow(() => effect.ApplyEffect(testPlayer));
    }

    [Test]
    public void ApplyEffect_CanBeCalledMultipleTimes_RaisesEventEachTime()
    {
        MoveToSpaceEventChannel channel = CreateChannel<MoveToSpaceEventChannel>();
        int callCount = 0;
        channel.Subscribe(_ => { callCount++; });

        var effect = TrackEffect(ScriptableObject.CreateInstance<MoveToSpaceCardEffect>());
        effect.moveToSpaceEventChannel = channel;
        effect.targetType = MoveToSpaceCardEffect.TargetSpaceType.PlanetSpace;

        effect.ApplyEffect(testPlayer);
        effect.ApplyEffect(testPlayer);
        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(3, callCount);
    }
}

}
