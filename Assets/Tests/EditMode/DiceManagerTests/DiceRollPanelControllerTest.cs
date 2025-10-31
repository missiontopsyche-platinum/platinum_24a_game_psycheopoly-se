using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class DiceRollPanelControllerTest
{
    private GameObject panelGO;
    private DiceRollPanelController panelController;
    private Text totalText;

    private FieldInfo totalTextField;
    private MethodInfo onDiceRolledMI;
    private MethodInfo animateTotalTextMI;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        //intantiate a canvas & controller
        var canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        panelGO = new GameObject("DicePanelController");
        panelController = panelGO.AddComponent<DiceRollPanelController>();

        var textGO = new GameObject("TotalText", typeof(Text));
        textGO.transform.SetParent(panelGO.transform, false);
        //text formatting
        totalText = textGO.GetComponent<Text>();
        totalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        totalText.text = "Total: -";

        //checker
        totalTextField = typeof(DiceRollPanelController)
            .GetField("totalText", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(totalTextField, "Could not find private field 'totalText' on DicePanelController.");

        onDiceRolledMI = typeof(DiceRollPanelController)
            .GetMethod("OnDiceRolled", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(onDiceRolledMI, "Could not find private method 'OnDiceRolled'.");

        animateTotalTextMI = typeof(DiceRollPanelController)
            .GetMethod("AnimateTotalText", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(animateTotalTextMI, "Could not find private method 'AnimateTotalText'.");

        totalTextField.SetValue(panelController, totalText);

        //make sure filed & instance are saem
        var fieldValue = (Text)totalTextField.GetValue(panelController);
        Assert.IsNotNull(fieldValue, "Injected totalText ended up null on the component.");
        Assert.AreSame(totalText, fieldValue, "Injected totalText is not the same instance stored on the component.");

        panelGO.SetActive(true);
        panelController.enabled = false;
        panelController.enabled = true;

        //1 frame buffer
        yield return null;
    }


    [UnityTest]
    public IEnumerator OnDiceRolled_UpdatesUILabel()
    {
        var fakeRoll = new DiceRolledEvent(3, 5, 8);

        //simulate OnDiceRolled
        string baseText = "Total: ";
        totalText.text = baseText;

        //mannually run the coroutine
        var animateMI = typeof(DiceRollPanelController)
            .GetMethod("AnimateTotalText", BindingFlags.NonPublic | BindingFlags.Instance);
        var enumerator = (IEnumerator)animateMI.Invoke(panelController, new object[] { fakeRoll.totalRoll });

        //another buffer yield to load everything
        yield return enumerator;

        Assert.AreEqual("Total: 8", totalText.text,
            $"Expected 'Total: 8' but got '{totalText.text}'.");
    }


    [UnityTearDown]
    public IEnumerator TearDown()
    {
#if UNITY_EDITOR
        UnityEngine.Object.DestroyImmediate(panelGO);
#else
        UnityEngine.Object.Destroy(panelGO);
#endif
        yield return null;
    }
}
