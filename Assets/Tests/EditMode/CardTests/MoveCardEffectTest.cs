using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MoveCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void MoveCardEffect_Forward_RaisesPositiveMove()
    {
        MovePlayerEventChannel moveChannel = CreateChannel<MovePlayerEventChannel>();
        List<MovePlayerEvent> raised = new();
        moveChannel.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<MoveCardEffect>());
        effect.Type = MoveCardEffect.EffectType.MoveForward;
        effect.SpacesToMove = 5;
        effect.MovePlayerEventChannel = moveChannel;

        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(1, raised.Count);
        Assert.AreEqual(testPlayer.GetId(), raised[0].id);
        Assert.AreEqual(5, raised[0].spacesToMove);
    }

    [Test]
    public void MoveCardEffect_Backward_RaisesNegativeMove()
    {
        MovePlayerEventChannel moveChannel = CreateChannel<MovePlayerEventChannel>();
        List<MovePlayerEvent> raised = new();
        moveChannel.Subscribe(e => raised.Add(e));

        var effect = TrackEffect(ScriptableObject.CreateInstance<MoveCardEffect>());
        effect.Type = MoveCardEffect.EffectType.MoveBackward;
        effect.SpacesToMove = 3;
        effect.MovePlayerEventChannel = moveChannel;

        effect.ApplyEffect(testPlayer);

        Assert.AreEqual(1, raised.Count);
        Assert.AreEqual(testPlayer.GetId(), raised[0].id);
        Assert.AreEqual(-3, raised[0].spacesToMove);
    }
}
