using Data;
using NUnit.Framework;
using Tests.EditMode.PlayerControllerTests.Builders;
using UnityEngine;

public class BankrupctyActionTests : BankrupctyActionTestBase
{
    [Test]
    public void BankrupctyAction_TrySpendReturnsBankrupt()
    {
        var player = Track(APlayer()
               .WithMoney(1000)
               .Build());

        Player.FinancialStatus result = player.TrySpend(1100);

        Assert.AreEqual(Player.FinancialStatus.Bankrupt, result);
    }
}
