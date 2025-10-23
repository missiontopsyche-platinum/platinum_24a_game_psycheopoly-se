using UnityEngine;
using UnityEngine.UI;

public class TurnBannerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text turnLabel;
    [SerializeField] private Button continueButton;

    [Header("Events")]
    [SerializeField] private TurnStartedEventChannel turnStartedChannel;

    private bool _hooked;

    //Exposed for testing purposes
    public bool IsSubscribed { get; private set; }

    private void Awake()
    {
        //Make sure hidden at startup 
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        HookButton();

        if (turnStartedChannel != null)
        {
            turnStartedChannel.Subscribe(OnTurnStarted);
            IsSubscribed = true;
        }
    }

    private void OnDisable()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
        _hooked = false;

        if (turnStartedChannel != null)
            turnStartedChannel.Unsubscribe(OnTurnStarted);

        IsSubscribed = false;
    }

    private void HookButton()
    {
        if (_hooked || continueButton == null) return;
        continueButton.onClick.AddListener(OnContinueClicked);
        _hooked = true;
    }

    private void OnTurnStarted(TurnStartedEvent payload)
    {
        ShowBanner(payload.playerId);
    }

    public void ShowBanner(int playerId)
    {
        if (turnLabel != null)
            turnLabel.text = $"Player {playerId}'s Turn";
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    private void OnContinueClicked() => Hide();

#if UNITY_EDITOR
    /// <summary>
    /// Helper for tests
    /// Keeps subscription state the same
    /// </summary>
    public void __InjectChannelForTests(TurnStartedEventChannel ch)
    {
        //If already enabled unhook it
        if (isActiveAndEnabled && turnStartedChannel != null)
            turnStartedChannel.Unsubscribe(OnTurnStarted);

        turnStartedChannel = ch;

        //If enabled when injecting then bind right away
        if (isActiveAndEnabled && turnStartedChannel != null)
        {
            turnStartedChannel.Subscribe(OnTurnStarted);
            IsSubscribed = true;
        }
        else
        {
            IsSubscribed = false;
        }
    }
#endif
}
