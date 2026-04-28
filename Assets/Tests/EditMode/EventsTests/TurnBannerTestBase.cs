using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

namespace Tests.EditMode.EventsTests
{
    public class TurnBannerTestBase : ManagerTestBase
    {
        protected GameObject canvasGO;
        protected Canvas canvas;

        protected GameObject bannerGO;
        protected RectTransform rect;
        protected Image background;
        protected CanvasGroup cg;
        protected TurnBannerController controller;

        protected GameObject labelGO;
        protected TextMeshProUGUI turnLabel;

        protected GameObject buttonGO;
        protected Button continueButton;

        protected TurnStartedEventChannel turnStartedEventChannel;

        protected virtual void CreateCanvas()
        {
            canvasGO = new GameObject("TurnBannerCanvas", typeof(Canvas));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [SetUp]
        public virtual void SetUpUI()
        {
            CreateCanvas();

            bannerGO = new GameObject(
                "TurnBanner",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(TurnBannerController)
            );

            bannerGO.transform.SetParent(canvasGO.transform, false);

            rect = bannerGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 300);
            rect.anchoredPosition = Vector2.zero;

            background = bannerGO.GetComponent<Image>();
            cg = bannerGO.GetComponent<CanvasGroup>();
            controller = bannerGO.GetComponent<TurnBannerController>();

            labelGO = new GameObject("TurnLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(bannerGO.transform, false);

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(.5f, .75f);
            labelRect.anchorMax = new Vector2(.5f, .75f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(500, 80);

            turnLabel = labelGO.GetComponent<TextMeshProUGUI>();
            turnLabel.alignment = TextAlignmentOptions.Center;
            turnLabel.text = "Player 1's Turn";
            turnLabel.fontSize = 36;

            buttonGO = new GameObject("ContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(bannerGO.transform, false);

            var btnRect = buttonGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(.5f, .35f);
            btnRect.anchorMax = new Vector2(.5f, .35f);
            btnRect.sizeDelta = new Vector2(220, 80);
            btnRect.anchoredPosition = Vector2.zero;

            var btnTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextGO.transform.SetParent(buttonGO.transform, false);

            var btnTextRect = btnTextGO.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            var btnText = btnTextGO.GetComponent<TextMeshProUGUI>();
            btnText.text = "Start Turn";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 28;

            continueButton = buttonGO.GetComponent<Button>();

            turnStartedEventChannel = ScriptableObject.CreateInstance<TurnStartedEventChannel>();

            SetPrivateField(controller, "turnLabel", turnLabel);
            SetPrivateField(controller, "continueButton", continueButton);
            SetPrivateField(controller, "canvasGroup", cg);
            TrySetPrivateField(controller, "turnStartedChannel", turnStartedEventChannel);
            TrySetPrivateField(controller, "currentCallback", new Action(() => { }));

            InvokePrivate(controller, "Awake");
            InvokePrivate(controller, "OnEnable");
        }

        [TearDown]
        public virtual void TearDownUI()
        {
            if (controller != null)
                InvokePrivate(controller, "OnDisable");

            if (turnStartedEventChannel != null)
                UnityEngine.Object.DestroyImmediate(turnStartedEventChannel);

            if (canvasGO != null)
                UnityEngine.Object.DestroyImmediate(canvasGO);
        }

        protected static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.NotNull(field, $"Missing field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        protected static void TrySetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (field == null)
                return;

            field.SetValue(target, value);
        }

        protected static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            method?.Invoke(target, null);
        }
    }
}