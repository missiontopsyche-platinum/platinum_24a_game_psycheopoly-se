using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CardPopupBlocksInputTests
{
    private const string TestSceneName = "CardPopupPlayModeTestScene";

    [UnityTest]
    public IEnumerator Popup_Blocks_Clicks_To_Underlying_UI()
    {
        //Load test scene
        yield return SceneManager.LoadSceneAsync(TestSceneName, LoadSceneMode.Single);

        var popup   = Object.FindFirstObjectByType<CardPopupUI>();
        var helper  = Object.FindFirstObjectByType<TestCardDrawHelper>();
        var buttonL = Object.FindFirstObjectByType<TestButtonClickListener>();

        Assert.IsNotNull(popup,   "CardPopupUI not found.");
        Assert.IsNotNull(helper,  "TestCardDrawHelper not found.");
        Assert.IsNotNull(buttonL, "TestButtonClickListener not found.");

        //Button should work when popup is hidden
        Assert.IsFalse(popup.IsVisible);
        Assert.AreEqual(0, buttonL.ClickCount);

        buttonL.SimulateClick();
        Assert.AreEqual(1, buttonL.ClickCount, "Button should respond when popup is hidden.");

        //Show a card
        helper.DrawOnce();

        //Wait for popup to be visible
        float timeout = 2f;
        float elapsed = 0f;
        while (!popup.IsVisible && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(popup.IsVisible, "Popup did not become visible.");

        //Try clicking the underlying button while popup is visible
        buttonL.SimulateClick();
        Assert.AreEqual(1, buttonL.ClickCount,
            "Click count should NOT increase while popup is visible (input must be blocked).");

        //Close popup
        popup.OkButton.onClick.Invoke();

        //Wait for popup to hide
        timeout = 2f;
        elapsed = 0f;
        while (popup.IsVisible && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsFalse(popup.IsVisible, "Popup did not hide after OK.");

        //Button should work again once popup is hidden
        buttonL.SimulateClick();
        Assert.AreEqual(2, buttonL.ClickCount,
            "Button should respond again after popup is hidden.");
    }
}
