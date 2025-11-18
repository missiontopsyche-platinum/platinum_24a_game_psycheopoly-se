using UnityEngine;
using UnityEngine.UI;

public class TestButtonClickListener : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private CardPopupUI popup;

    public int ClickCount { get; private set; }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (popup == null)
            popup = FindFirstObjectByType<CardPopupUI>();

        if (button != null)
            button.onClick.AddListener(OnClicked);
        else
            Debug.LogError("TestButtonClickListener: Button reference is missing.");
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        ClickCount++;
        Debug.Log($"[TestButtonClickListener] OnClicked, ClickCount = {ClickCount}");
    }

    public void SimulateClick()
    {
        if (button == null)
            return;

        //If popup is visible then pretend the click is blocked
        if (popup != null && popup.IsVisible)
        {
            Debug.Log("[TestButtonClickListener] Click blocked because popup is visible.");
            return;
        }

        Debug.Log("[TestButtonClickListener] SimulateClick -> invoking button.onClick");
        button.onClick.Invoke();
    }
}
