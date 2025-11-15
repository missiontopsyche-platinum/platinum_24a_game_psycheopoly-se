using NUnit.Framework;
using System.Collections.Generic;

public class PayAndCollectFromAllPlayersTest : CardEffectBaseTest
{
    [Test]
    public void PayAllPlayers_ChargesInitiator()
    {
        InitializePlayerManagerChannels();
        var pay = playerManager.payAllPlayersEventChannel;
        var payments = new List<MoneyDistributionEvent>();
        pay.Subscribe(e => payments.Add(e));

        var effect = new PayAllPlayersCardEffect
        {
            Amount = 25,
            payAllPlayersEventChannel = pay,
        };

        // Player A pays everyone else 25
        effect.ApplyEffect(playerA);

        // Just to make sure events fire
        Assert.AreEqual(1, payments.Count);
        Assert.AreEqual(0, payments[0].Player.GetId());
        Assert.AreEqual(25, payments[0].Amount);

        // Confirm amount of money
        Assert.AreEqual(1525, playerB.GetMoney());
        Assert.AreEqual(1525, playerC.GetMoney());
    }

    [Test]
    public void CollectFromAllPlayers_ChargesOthers()
    {
        InitializePlayerManagerChannels();
        var charge = playerManager.collectFromAllPlayersEventChannel;
        var charges = new List<MoneyDistributionEvent>();
        charge.Subscribe(e => charges.Add(e));

        var effect = new PayAllPlayersCardEffect
        {
            Amount = 25,
            payAllPlayersEventChannel = charge,
        };

        effect.ApplyEffect(playerA);

        Assert.AreEqual(1, charges.Count);
        Assert.AreEqual(0, charges[0].Player.GetId());
        Assert.AreEqual(25, charges[0].Amount);

        Assert.AreEqual(1475, playerB.GetMoney());
        Assert.AreEqual(1475, playerC.GetMoney());
    }
}
