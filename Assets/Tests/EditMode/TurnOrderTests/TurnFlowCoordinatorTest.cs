using Assets.Scripts.Managers.TurnFlow;
using NUnit.Framework;
using Tests.EditMode;
using UnityEngine;

public class TurnFlowCoordinatorTest : ManagerTestBase
{
    private GameObject go;
    private TurnFlowCoordinator tfc;

    private TurnActionResultEventChannel resultChannel;
    private TurnActionRequestEventChannel requestChannel;

    private System.Action<TurnActionResult> resultHandler;
    private TurnActionResult lastResult;
    private int resultCount;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject();
        tfc = go.AddComponent<TurnFlowCoordinator>();

        resultChannel = CreateChannel<TurnActionResultEventChannel>();
        requestChannel = CreateChannel<TurnActionRequestEventChannel>();

        SetPrivateField(tfc, "turnActionResultChannel", resultChannel);
        SetPrivateField(tfc, "turnActionRequestChannel", requestChannel);

        lastResult = default;
        resultCount = 0;
        resultChannel.Subscribe(r => { lastResult = r; resultCount++; });
        resultChannel.Subscribe(resultHandler);

        tfc.enabled = true;
        tfc.gameObject.SetActive(true);

        SetProperty(tfc, "ActivePlayer", 0);
        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingRoll);

    }

    [TearDown]
    public void TearDown()
    {
        if (resultChannel != null && resultHandler != null)
            resultChannel.Unsubscribe(resultHandler);

        Object.DestroyImmediate(resultChannel);
        Object.DestroyImmediate(requestChannel);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void OnTurnActionRequested_RollDice_AllowedOnlyWhenAwaitingRoll()
    {
        var p = ScriptableObject.CreateInstance<Player>();
        p.SetId(0);

        SetProperty(tfc, "ActivePlayer", 0);
        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingRoll);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = p, action = TurnActionType.RollDice });

        Assert.AreEqual(1, resultCount);
        Assert.AreEqual(0, lastResult.playerId);
        Assert.AreEqual(TurnActionType.RollDice, lastResult.action);
        Assert.IsTrue(lastResult.allowed);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingMovement);
        tfc.TurnActionRequestTest(new TurnActionRequest { player = p, action = TurnActionType.RollDice });

        Assert.AreEqual(2, resultCount);
        Assert.IsFalse(lastResult.allowed);

        var other = ScriptableObject.CreateInstance<Player>();
        other.SetId(1);
        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingRoll);
        tfc.TurnActionRequestTest(new TurnActionRequest { player = other, action = TurnActionType.RollDice });

        Assert.AreEqual(3, resultCount);
        Assert.IsTrue(lastResult.allowed);

        Object.DestroyImmediate(p);
        Object.DestroyImmediate(other);
    }

    [Test]
    public void OnTurnActionRequested_BuyProperty_AllowedOnlyWhenAwaitingResolution_AndActivePlayer()
    {
        var p = ScriptableObject.CreateInstance<Player>();
        p.SetId(0);

        SetProperty(tfc, "ActivePlayer", 0);
        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingResolution);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = p, action = TurnActionType.BuyProperty });

        Assert.AreEqual(1, resultCount);
        Assert.IsTrue(lastResult.allowed);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.None);
        tfc.TurnActionRequestTest(new TurnActionRequest { player = p, action = TurnActionType.BuyProperty });

        Assert.AreEqual(2, resultCount);
        Assert.IsFalse(lastResult.allowed);

        Object.DestroyImmediate(p);
    }

    //test early CompleteResolution is deferred
    [Test]
    public void CompleteResolution_WhenRequestedDuringAwaitingMovement_DefersUntilMovementResolutionCompletes()
    {
        var p = ScriptableObject.CreateInstance<Player>();
        p.SetId(0);

        SetProperty(tfc, "ActivePlayer", 0);
        SetTfcPhase("AwaitingMovement");

        tfc.TurnActionRequestTest(new TurnActionRequest
        {
            player = p,
            action = TurnActionType.CompleteResolution
        });

        Assert.AreEqual(1, resultCount);
        Assert.IsTrue(lastResult.allowed);
        Assert.AreEqual("AwaitingMovement", tfc.Phase.ToString(),
            "Phase should stay AwaitingMovement because CompleteResolution should be deferred.");

        SimulateMovementResolutionCompleted();

        Assert.AreEqual("Completed", tfc.Phase.ToString(),
            "Deferred CompleteResolution should complete after movement resolution finishes.");

        Object.DestroyImmediate(p);
    }

    //jailed player can defer CompleteResolutin
    [Test]
    public void JailedPlayer_CompleteResolutionDuringAwaitingMovement_IsAllowedAndDeferred()
    {
        var p = ScriptableObject.CreateInstance<Player>();
        p.SetId(0);
        p.SetInJail(true);

        SetProperty(tfc, "ActivePlayer", 0);
        SetTfcPhase("AwaitingMovement");

        tfc.TurnActionRequestTest(new TurnActionRequest
        {
            player = p,
            action = TurnActionType.CompleteResolution
        });

        Assert.AreEqual(1, resultCount);
        Assert.IsTrue(lastResult.allowed,
            "Jailed players must be allowed to request CompleteResolution during AwaitingMovement.");

        Assert.AreEqual("AwaitingMovement", tfc.Phase.ToString(),
            "CompleteResolution should not complete immediately while movement is still resolving.");

        SimulateMovementResolutionCompleted();

        Assert.AreEqual("Completed", tfc.Phase.ToString(),
            "After movement resolution completes, the pending CompleteResolution should finish the turn resolution.");

        Object.DestroyImmediate(p);
    }

    //completeResolution is still denied too early in the turn
    [Test]
    public void CompleteResolution_WhenRequestedDuringAwaitingRoll_IsDenied()
    {
        var p = ScriptableObject.CreateInstance<Player>();
        p.SetId(0);

        SetProperty(tfc, "ActivePlayer", 0);
        SetTfcPhase("AwaitingRoll");

        tfc.TurnActionRequestTest(new TurnActionRequest
        {
            player = p,
            action = TurnActionType.CompleteResolution
        });

        Assert.AreEqual(1, resultCount);
        Assert.IsFalse(lastResult.allowed);
        Assert.AreEqual("AwaitingRoll", tfc.Phase.ToString());

        Object.DestroyImmediate(p);
    }

    //CompleteResolution still works normal during AwaitingResolution
    [Test]
    public void CompleteResolution_WhenRequestedDuringAwaitingResolution_CompletesImmediately()
    {
        var p = ScriptableObject.CreateInstance<Player>();
        p.SetId(0);

        SetProperty(tfc, "ActivePlayer", 0);
        SetTfcPhase("AwaitingResolution");

        tfc.TurnActionRequestTest(new TurnActionRequest
        {
            player = p,
            action = TurnActionType.CompleteResolution
        });

        Assert.AreEqual(1, resultCount);
        Assert.IsTrue(lastResult.allowed);
        Assert.AreEqual("Completed", tfc.Phase.ToString());

        Object.DestroyImmediate(p);
    }

    [Test]
    public void OnTurnActionRequested_UpgradeProperty_AllowUpgradeAtAnyPointDuringTurn()
    {

        var active = ScriptableObject.CreateInstance<Player>();
        active.SetId(0);

        var other = ScriptableObject.CreateInstance<Player>();
        other.SetId(1);

        SetProperty(tfc, "ActivePlayer", 0);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.None);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = other, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(1, resultCount);
        Assert.IsFalse(lastResult.allowed);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingRoll);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = other, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(2, resultCount);
        Assert.IsTrue(lastResult.allowed);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = active, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(3, resultCount);
        Assert.IsTrue(lastResult.allowed);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingMovement);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = other, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(4, resultCount);
        Assert.IsTrue(lastResult.allowed);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = active, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(5, resultCount);
        Assert.IsTrue(lastResult.allowed);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.AwaitingResolution);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = other, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(6, resultCount);
        Assert.IsTrue(lastResult.allowed);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = active, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(7, resultCount);
        Assert.IsTrue(lastResult.allowed);

        SetProperty(tfc, "Phase", Assets.Scripts.Managers.TurnFlow.TurnPhase.Completed);
        tfc.SetAwaitingEndTurn(true);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = other, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(8, resultCount);
        Assert.IsTrue(lastResult.allowed);

        tfc.TurnActionRequestTest(new TurnActionRequest { player = active, action = TurnActionType.ModifyProperty });
        Assert.AreEqual(9, resultCount);
        Assert.IsTrue(lastResult.allowed);

        Object.DestroyImmediate(active);
        Object.DestroyImmediate(other);
    }

    // Helpers to set private fields and props.
    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var f = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(f, $"Missing field '{fieldName}' on {obj.GetType().Name}");
        f.SetValue(obj, value);
    }

    private static void SetProperty(object obj, string propName, object value)
    {
        var p = obj.GetType().GetProperty(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(p, $"Missing property '{propName}' on {obj.GetType().Name}");

        var backing = obj.GetType().GetField($"<{propName}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(backing, $"Missing backing field for property '{propName}' on {obj.GetType().Name}");
        backing.SetValue(obj, value);
    }

    private void SetTfcPhase(string phaseName)
    {
        var phaseType = tfc.Phase.GetType();
        object parsedPhase = System.Enum.Parse(phaseType, phaseName);
        SetProperty(tfc, "Phase", parsedPhase);
    }

    private void SimulateMovementResolutionCompleted()
    {
        var method = typeof(TurnFlowCoordinator).GetMethod(
            "OnPieceMoveCompleted",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(method, "Missing private method OnPieceMoveCompleted on TurnFlowCoordinator.");

        method.Invoke(tfc, new object[] { true });
    }
}
