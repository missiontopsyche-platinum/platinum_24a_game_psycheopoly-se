using System.Text;
using Events.EventDataStructures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnHoverUI : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private SpaceHoverEventChannel spaceHoverEventChannel;
    [SerializeField] private BooleanEventChannel onSpaceExitEventChannel;

    // Runtime-built UI
    private GameObject panelRoot;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private Image headerColorImage;

    private void Awake()
    {
        EnsureUIBuilt();
        SetVisible(false);
    }

    private void OnEnable()
    {
        spaceHoverEventChannel?.Subscribe(OnSpaceHovered);
        onSpaceExitEventChannel?.Subscribe(OnSpaceExit);
    }

    private void OnDisable()
    {
        spaceHoverEventChannel?.Unsubscribe(OnSpaceHovered);
        onSpaceExitEventChannel?.Unsubscribe(OnSpaceExit);
    }

    private void EnsureUIBuilt()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("OnHoverUI: No Canvas found on this GameObject. HoverPanelUI prefab must have a Canvas.");
            return;
        }

        //create/find PanelRoot
        var existingPanel = transform.Find("PanelRoot");
        if (existingPanel != null)
        {
            panelRoot = existingPanel.gameObject;
        }
        else
        {
            panelRoot = new GameObject("PanelRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelRoot.transform.SetParent(transform, false);

            var rt = panelRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.65f, 0.05f);
            rt.anchorMax = new Vector2(0.98f, 0.35f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = panelRoot.GetComponent<Image>();
            bg.raycastTarget = false;
            bg.color = new Color(0f, 0f, 0f, 0.75f);

            // Header color bar (optional)
            var header = new GameObject("HeaderBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            header.transform.SetParent(panelRoot.transform, false);
            headerColorImage = header.GetComponent<Image>();
            headerColorImage.raycastTarget = false;

            var hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 0.85f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;

            // Title TMP
            titleText = CreateTMP("TitleTMP", panelRoot.transform, anchorMin: new Vector2(0.03f, 0.68f), anchorMax: new Vector2(0.97f, 0.85f), fontSize: 28);

            //Body TMP
            bodyText = CreateTMP("BodyTMP", panelRoot.transform, anchorMin: new Vector2(0.03f, 0.05f), anchorMax: new Vector2(0.97f, 0.66f), fontSize: 20);


        }

        if (headerColorImage == null)
        {
            var header = panelRoot.transform.Find("HeaderBar");
            if (header != null) headerColorImage = header.GetComponent<Image>();
        }
        if (titleText == null)
        {
            var t = panelRoot.transform.Find("TitleTMP");
            if (t != null) titleText = t.GetComponent<TextMeshProUGUI>();
        }
        if (bodyText == null)
        {
            var b = panelRoot.transform.Find("BodyTMP");
            if (b != null) bodyText = b.GetComponent<TextMeshProUGUI>();
        }
    }

    private TextMeshProUGUI CreateTMP(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.raycastTarget = false;
        tmp.fontSize = fontSize;
        tmp.enableWordWrapping = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.text = "";

        return tmp;
    }

    private void OnSpaceHovered(SpaceHoverEvent evt)
    {
        if (evt == null) return;

        EnsureUIBuilt();
        if (panelRoot == null) return;

        if (titleText != null) titleText.text = evt.spaceName;
        if (headerColorImage != null) headerColorImage.color = evt.spaceColor;

        if (bodyText != null)
        {
            var sb = new StringBuilder();
            if (evt.spaceInformation != null)
            {
                for (int i = 0; i < evt.spaceInformation.Count; i++)
                    sb.AppendLine(evt.spaceInformation[i]);
            }
            bodyText.text = sb.ToString().TrimEnd();
        }

        SetVisible(true);
    }

    private void OnSpaceExit(bool _)
    {
        SetVisible(false);
    }

    private void SetVisible(bool isVisible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(isVisible);
    }
}
