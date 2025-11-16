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

            // ⬇️ instead of WaitUntil (which can hang), poll with a timeout
            float timeout = 2f;
            float elapsed = 0f;
            while (!popup.gameObject.activeSelf && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(popup.gameObject.activeSelf,
                $"Popup never became active after draw #{i + 1}. " +
                "If this fails, check that CardDeck raises the CardDrawn event and that the popup is subscribed.");

            // click OK
            popup.OkButton.onClick.Invoke();

            // wait for fade-out to finish (adjust if your fadeDuration is different)
            yield return new WaitForSeconds(0.35f);

            Assert.IsFalse(popup.gameObject.activeSelf,
                $"Popup should be hidden after clicking OK on draw #{i + 1}.");
        }
    }
}
