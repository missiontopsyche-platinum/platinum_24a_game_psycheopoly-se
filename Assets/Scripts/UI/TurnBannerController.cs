using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnBannerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text turnLabel;
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Events")]
    [SerializeField] private TurnStartedEventChannel turnStartedChannel;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float slideOffset = 60f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _hooked;
    private Vector2 _shownPos;
    private Vector2 _hiddenPos;
    private RectTransform _rect;
    private Coroutine _anim;

    //Exposed for testing purposes
    public bool IsSubscribed { get; private set; }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        //Cache slide positions
        _shownPos = _rect.anchoredPosition;
        _hiddenPos = _shownPos + new Vector2(0, -slideOffset);

        //Make sure initial hidden hidden
        canvasGroup.alpha = 0f;
        _rect.anchoredPosition = _hiddenPos;
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
        PlayAnim(show: true);
    }

      public void Hide()
    {
        PlayAnim(show: false);
    }

    private void OnContinueClicked()
    {
        Hide();
    }

    private void PlayAnim(bool show)
    {
        if (_anim != null)
            StopCoroutine(_anim);
        _anim = StartCoroutine(FadeSlide(show));
    }

    private IEnumerator FadeSlide(bool show)
    {
        float duration = Mathf.Max(0.01f, fadeDuration);
        float time = 0f;

        //target values
        float startAlpha = canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        Vector2 startPos = _rect.anchoredPosition;
        Vector2 endPos = show ? _shownPos : _hiddenPos;

        //if showing, make sure object is active and starts hidden
        if (show && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            _rect.anchoredPosition = _hiddenPos;
            startAlpha = 0f;
            startPos = _hiddenPos;
        }

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = ease.Evaluate(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        _rect.anchoredPosition = endPos;

        if (!show)
            gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Helper for tests
    /// Keeps subscription state the same
    /// </summary>
    public void __InjectChannelForTests(TurnStartedEventChannel ch)
    {
        if (isActiveAndEnabled && turnStartedChannel != null)
            turnStartedChannel.Unsubscribe(OnTurnStarted);

        turnStartedChannel = ch;

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
