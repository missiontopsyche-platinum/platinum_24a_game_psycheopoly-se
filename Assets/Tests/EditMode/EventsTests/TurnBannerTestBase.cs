using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tests.EditMode.EventsTests
{
    /// <summary>
    /// Builds a TurnBanner hierarchy and wires for testing
    ///Uses existing TurnBannerController
    /// </summary>
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
        protected Text turnLabel;

        protected GameObject buttonGO;
        protected Button continueButton;

        protected TurnStartedEventChannel turnStartedEventChannel;

        /// <summary>Creates a canvas so UI components can work in EditMode.</summary>
        protected virtual void CreateCanvas()
        {
            canvasGO = new GameObject("TurnBannerCanvas", typeof(Canvas));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [SetUp]
        public virtual void SetUpUI()
        {
            //Canvas
            CreateCanvas();

            //Banner root
            bannerGO = new GameObject("TurnBanner",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(TurnBannerController));

            bannerGO.transform.SetParent(canvasGO.transform, false);

            rect = bannerGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 300);
            rect.anchoredPosition = Vector2.zero;

            background = bannerGO.GetComponent<Image>();
            cg = bannerGO.GetComponent<CanvasGroup>();
            controller = bannerGO.GetComponent<TurnBannerController>();

            //Text label
            labelGO = new GameObject("TurnLabel", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(bannerGO.transform, false);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(.5f, .75f);
            labelRect.anchorMax = new Vector2(.5f, .75f);
            labelRect.anchoredPosition = Vector2.zero;

            turnLabel = labelGO.GetComponent<Text>();
            turnLabel.alignment = TextAnchor.MiddleCenter;
            turnLabel.text = "Player 1's Turn";
            turnLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            turnLabel.fontSize = 36;

            //Continue Button and text
            buttonGO = new GameObject("ContinueButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(bannerGO.transform, false);
            var btnRect = buttonGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(.5f, .35f);
            btnRect.anchorMax = new Vector2(.5f, .35f);
            btnRect.sizeDelta = new Vector2(220, 80);
            btnRect.anchoredPosition = Vector2.zero;

            var btnTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btnTextGO.transform.SetParent(buttonGO.transform, false);
            var btnText = btnTextGO.GetComponent<Text>();
            btnText.text = "Start Turn";
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 28;
            var btnTextRect = btnTextGO.GetComponent<RectTransform>();
            btnTextRect.anchorMin = btnTextRect.anchorMax = new Vector2(.5f, .5f);
            btnTextRect.sizeDelta = Vector2.zero;

            continueButton = buttonGO.GetComponent<Button>();

            turnStartedEventChannel = ScriptableObject.CreateInstance<TurnStartedEventChannel>();

            //Wire refs to controller
            controller.GetType().GetField("turnLabel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(controller, turnLabel);

            controller.GetType().GetField("continueButton",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(controller, continueButton);

            controller.GetType().GetField("canvasGroup",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(controller, cg);
            
            controller.GetType().GetField("turnStartedChannel",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(controller, turnStartedEventChannel);

            //Manually simulate Awake() because they will not fire in EditMode testing 
            var awake = controller.GetType().GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            awake?.Invoke(controller, null);

            //OnEnable() to hook button listener
            var onEnable = controller.GetType().GetMethod("OnEnable",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            onEnable?.Invoke(controller, null);



        }
    }
}
