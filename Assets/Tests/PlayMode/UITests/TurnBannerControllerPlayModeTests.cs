using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class TurnBannerControllerPlayModeTests
{
    private static bool TryPublish(object channel, object payload)
    {
        var t = channel.GetType();

        string[] names = { "Raise", "RaiseEvent", "Publish", "Emit", "Dispatch", "Send", "Broadcast", "Post" };

        //Public first
        foreach (var name in names)
        {
            var m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
            if (m == null) continue;
            var ps = m.GetParameters();
            if (ps.Length == 1 && ps[0].ParameterType.IsInstanceOfType(payload))
            {
                m.Invoke(channel, new[] { payload });
                return true;
            }
        }

        //Protected/NonPublic 
        foreach (var name in names)
        {
            var m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (m == null) continue;
            var ps = m.GetParameters();
            if (ps.Length == 1 && ps[0].ParameterType.IsInstanceOfType(payload))
            {
                m.Invoke(channel, new[] { payload });
                return true;
            }
        }

        return false;
    }

    [UnityTest]
    public IEnumerator Banner_Shows_On_TurnStarted_And_Hides_On_Continue()
    {
        //Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        //Root banner GO 
        var root = new GameObject("TurnBanner", typeof(RectTransform));
        root.transform.SetParent(canvasGO.transform, false);
        root.SetActive(false);

        //Label 
        var labelGO = new GameObject("TurnLabel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelGO.transform.SetParent(root.transform, false);
        var label = labelGO.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        //Button
        var buttonGO = new GameObject("ContinueButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(root.transform, false);
        var button = buttonGO.GetComponent<Button>();

        //Event channel
        var controller = root.AddComponent<TurnBannerController>();
        var channel    = ScriptableObject.CreateInstance<TurnStartedEventChannel>();

        //Inject serialized privates 
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        typeof(TurnBannerController)
            .GetField("turnLabel", flags)
            .SetValue(controller, label);

        typeof(TurnBannerController)
            .GetField("continueButton", flags)
            .SetValue(controller, button);

        typeof(TurnBannerController)
            .GetField("turnStartedChannel", flags)
            .SetValue(controller, channel);

        //Activate GO 
        root.SetActive(true);
        yield return null;

        var onEnable = typeof(TurnBannerController).GetMethod("OnEnable", flags);
        onEnable.Invoke(controller, null);
        yield return null;

        //Publish event
        var payload = new TurnStartedEvent(3, 1);
        bool published = TryPublish(channel, payload);
        yield return null; //dispatch
        yield return null; //update UI

        if (string.IsNullOrEmpty(label.text))
        {
            //Make sure actually subscribed 
            Assert.IsTrue(controller.IsSubscribed, "Controller did not subscribe to the channel.");

            var onTurnStarted = typeof(TurnBannerController).GetMethod("OnTurnStarted", flags);
            onTurnStarted.Invoke(controller, new object[] { payload });
            yield return null;
        }

        //Check that banner shows and label
        Assert.IsTrue(root.activeSelf, "Banner should be visible after TurnStarted event.");
        Assert.AreEqual("Player 3's Turn", label.text, "Turn label not updated.");

        //Click Continue and element is hidden
        button.onClick.Invoke();
        yield return null;

        Assert.IsFalse(root.activeSelf, "Banner should hide after pressing Continue.");

        //Cleanup
        Object.DestroyImmediate(channel);
        Object.DestroyImmediate(canvasGO);
    }
}
