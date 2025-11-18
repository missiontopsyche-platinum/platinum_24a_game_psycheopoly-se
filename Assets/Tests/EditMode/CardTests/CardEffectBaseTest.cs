using Assets.Scripts.Events.EventChannelTypes;
using NUnit.Framework;
using PsycheOpoly.Board;
using Tests.EditMode;
using UnityEngine;

public class CardEffectBaseTest : ManagerTestBase
{
    protected GameObject playerManagerGO;
    protected GameObject boardManagerGO;
    protected PlayerManager playerManager;
    protected BoardManager boardManager;
    protected Player playerA;
    protected Player playerB;
    protected Player playerC;

    [SetUp]
    protected void SetUp()
    {
        InitializeTestLogger();

        playerManagerGO = new GameObject();
        playerManager = playerManagerGO.AddComponent<PlayerManager>();
        playerManager.InitializePlayers(3);
        var roster = playerManager.GetAllPlayers();

        playerA = roster[0];
        playerA.SetPName("Player A");
        playerA.SetMoney(1500);

        playerB = roster[1];
        playerB.SetPName("Player B");
        playerB.SetMoney(1500);

        playerC = roster[2];
        playerC.SetPName("Player C");
        playerC.SetMoney(1500);

        boardManagerGO = new GameObject();
        boardManager = boardManagerGO.AddComponent<BoardManager>();
    }

    [TearDown]
    public void TearDown()
    {
        DestroyTestObjects(playerA, playerB, playerC);
    }

    protected void InitializePlayerManagerChannels()
    {
        ChargePlayerEventChannel charge = CreateChannel<ChargePlayerEventChannel>();
        playerManager.chargePlayerEventChannel = charge;
        charge.Subscribe(playerManager.OnChargePlayerEvent);

        PayPlayerEventChannel pay = CreateChannel<PayPlayerEventChannel>();
        playerManager.payPlayerEventChannel = pay;
        pay.Subscribe(playerManager.OnPayPlayerEvent);

        JailStateChangedEventChannel jail = CreateChannel<JailStateChangedEventChannel>();
        playerManager.jailStateChangedEventChannel = jail;
        jail.Subscribe(playerManager.OnJailStateChangedEvent);

        MoneyDistributionEventChannel payAll = CreateChannel<MoneyDistributionEventChannel>();
        playerManager.payAllPlayersEventChannel = payAll;
        payAll.Subscribe(playerManager.OnPayAllPlayersEvent);

        MoneyDistributionEventChannel chargeAll = CreateChannel<MoneyDistributionEventChannel>();
        playerManager.collectFromAllPlayersEventChannel = chargeAll;
        chargeAll.Subscribe(playerManager.OnChargeAllPlayersEvent);
    }

    protected void InitializeBoardManagerChannels() 
    {
        MovePlayerEventChannel move = CreateChannel<MovePlayerEventChannel>();
        boardManager.movePlayerChannel = move;
        move.Subscribe(boardManager.MovePlayer);

        MoveToSpaceEventChannel moveTo = CreateChannel<MoveToSpaceEventChannel>();
        boardManager.moveToSpaceEventChannel = moveTo;
        moveTo.Subscribe(boardManager.OnMoveToSpaceEvent);
    }
}

