using UnityEngine;
using UnityEngine.UI;

public class TestButtonClickListener : MonoBehaviour
{
    [SerializeField] private Button button;

    public int ClickCount { get; private set; }

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        ClickCount++;
    }

    //Used from tests
    public void SimulateClick()
    {
        if (button != null)
            button.onClick.Invoke();
    }
}
