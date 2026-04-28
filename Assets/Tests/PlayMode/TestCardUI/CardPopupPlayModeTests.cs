using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class CardPopupPlayModeTests
{
    [UnityTest]
    public IEnumerator Popup_ShowsOnDraw_ClosesOnOk_Repeats()
    {
        var popupObject = new GameObject("TestCardPopup");
        popupObject.SetActive(false);

        var canvasGroup = popupObject.AddComponent<CanvasGroup>();

        var buttonObject = new GameObject("OkButton");
        buttonObject.transform.SetParent(popupObject.transform);
        var okButton = buttonObject.AddComponent<Button>();

        var popup = popupObject.AddComponent<CardPopupUI>();

        typeof(CardPopupUI)
            .GetField("canvasGroup", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(popup, canvasGroup);

        typeof(CardPopupUI)
            .GetField("okButton", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(popup, okButton);

        popupObject.SetActive(true);
        yield return null;

        const int cycles = 3;

        for (int i = 0; i < cycles; i++)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            yield return null;

            Assert.IsTrue(popup.IsVisible,
                $"Popup never became visible after draw #{i + 1}.");

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            yield return null;

            Assert.IsFalse(popup.IsVisible,
                $"Popup did not hide after clicking OK on draw #{i + 1}.");
        }

        Object.Destroy(popupObject);
    }
}