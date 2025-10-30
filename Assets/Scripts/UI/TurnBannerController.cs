using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnBannerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text turnLabel;         
    [SerializeField] private Button continueButton;   //Start turn button
    [SerializeField] private CanvasGroup canvasGroup; //Controls fade

    [Header("Events")]
    [SerializeField] private TurnStartedEventChannel turnStartedChannel;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float slideOffset = 60f;
    [SerializeField] private AnimationCurve ease = null;

    private RectTransform _rect;
    private Vector2 _shownPos;
    private Vector2 _hiddenPos;
    private Coroutine _anim;
    private bool _buttonHooked;
    public bool IsSubscribed { get; private set; }

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (ease == null || ease.length == 0)
            ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        _shownPos = _rect.anchoredPosition;
        _hiddenPos = _shownPos + new Vector2(0, -slideOffset);

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _rect.anchoredPosition = _hiddenPos;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        //hook button
        if (!_buttonHooked && continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            _buttonHooked = true;
        }

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

        if (turnStartedChannel != null)
            turnStartedChannel.Unsubscribe(OnTurnStarted);

        IsSubscribed = false;
    }

    private void OnTurnStarted(TurnStartedEvent payload)
    {
        turnLabel.text = $"Player {payload.playerId}'s Turn";
        Show();
    }

    private void OnContinueClicked()
    {
        Hide();
    }

    public void Show() => PlayAnim(true);
    public void Hide() => PlayAnim(false);

    private void PlayAnim(bool show)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(FadeSlide(show));
    }

    private IEnumerator FadeSlide(bool show)
    {
        if (!Application.isPlaying)
        {
            float targetAlpha = show ? 1f : 0f;
            Vector2 targetPos = show ? _shownPos : _hiddenPos;

            canvasGroup.alpha = targetAlpha;
            _rect.anchoredPosition = targetPos;

            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;

            if (!show)
                gameObject.SetActive(false);

            yield break;
        }
                
        float time = 0;
        float duration = Mathf.Max(0.01f, fadeDuration);

        float startA = canvasGroup.alpha;
        float endA = show ? 1 : 0;

        Vector2 startPos = _rect.anchoredPosition;
        Vector2 endPos = show ? _shownPos : _hiddenPos;

        if (show && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            _rect.anchoredPosition = _hiddenPos;
            startA = 0;
            startPos = _hiddenPos;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = ease.Evaluate(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startA, endA, t);
            _rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        canvasGroup.alpha = endA;
        _rect.anchoredPosition = endPos;

        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;

        if (!show)
            gameObject.SetActive(false);
    }

    //shows Player X's Turn
    public void ShowBanner(int playerId)
    {
        if (turnLabel != null)
            turnLabel.text = $"Player {playerId}'s Turn";

        gameObject.SetActive(true);
        PlayAnim(true);
    }

    //Could be used to show turn number
    public void ShowTurn(int playerId, int turnNum)
    {
        if (turnLabel != null)
            turnLabel.text = $"Turn {turnNum} — Player {playerId}";

        gameObject.SetActive(true);
        PlayAnim(true);
    }


}
