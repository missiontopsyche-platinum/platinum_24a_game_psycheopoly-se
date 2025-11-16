using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CardPopupPlayModeTests
{
    private const string TestSceneName = "CardPopupPlayModeTestScene";

    [UnityTest]
    public IEnumerator Popup_ShowsOnDraw_ClosesOnOk_Repeats()
    {
        // 1. Load the test scene
        yield return SceneManager.LoadSceneAsync(TestSceneName, LoadSceneMode.Single);

        // 2. Find popup + helper
        var popup = Object.FindObjectOfType<CardPopupUI>();
        Assert.IsNotNull(popup, "CardPopupUI not found in scene.");

        var helper = Object.FindObjectOfType<TestCardDrawHelper>();
        Assert.IsNotNull(helper, "TestCardDrawHelper not found in scene.");

        const int cycles = 3;

        for (int i = 0; i < cycles; i++)
{
    helper.DrawOnce();

    // wait up to 2 seconds for popup to become visible
    float timeout = 2f;
    float elapsed = 0f;
    while (!popup.IsVisible && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }

    Assert.IsTrue(popup.IsVisible,
        $"Popup never became visible after draw #{i + 1}. " +
        "If this fails, check that CardDeck raises the CardDrawn event and that the popup is subscribed.");

    // simulate OK click
    popup.OkButton.onClick.Invoke();

    // wait for fade-out
    timeout = 2f;
    elapsed = 0f;
    while (popup.IsVisible && elapsed < timeout)
    {
        elapsed += Time.deltaTime;
        yield return null;
    }

    Assert.IsFalse(popup.IsVisible,
        $"Popup did not hide after clicking OK on draw #{i + 1}.");
}
    }
}
