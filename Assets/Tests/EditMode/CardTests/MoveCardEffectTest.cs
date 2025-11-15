using NUnit.Framework;
using System.Collections.Generic;

public class MoveCardEffectTest : CardEffectBaseTest
{
    [Test]
    public void MoveCardEffect_Forward_RaisesPositiveMove()
    {
        InitializeBoardManagerChannels();
        var moveChannel = boardManager.movePlayerChannel;
        var raised = new List<MovePlayerEvent>();
        moveChannel.Subscribe(e => raised.Add(e));

        var effect = new MoveCardEffect
        {
            Type = MoveCardEffect.EffectType.MoveForward,
            SpacesToMove = 5,
            MovePlayerEventChannel = moveChannel
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, raised.Count, "Expected exactly one MovePlayerEvent.");
        Assert.AreEqual(playerA.GetId(), raised[0].id);
        Assert.AreEqual(5, raised[0].spacesToMove);
        Assert.AreEqual(5, boardManager.GetPlayerPosition(playerA.GetId()));
    }

    [Test]
    public void MoveCardEffect_Backward_RaisesNegativeMove()
    {
        InitializeBoardManagerChannels();
        var moveChannel = boardManager.movePlayerChannel;
        var raised = new List<MovePlayerEvent>();
        moveChannel.Subscribe(e => raised.Add(e));

        var effect = new MoveCardEffect
        {
            Type = MoveCardEffect.EffectType.MoveBackward,
            SpacesToMove = 3,
            MovePlayerEventChannel = moveChannel
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, raised.Count);
        Assert.AreEqual(playerA.GetId(), raised[0].id);
        Assert.AreEqual(-3, raised[0].spacesToMove);
        Assert.AreEqual(37, boardManager.GetPlayerPosition(playerA.GetId()));
    }
}
