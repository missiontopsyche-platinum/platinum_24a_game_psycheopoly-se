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

    private RectTransform rect;
    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private Coroutine anim;
    private bool _buttonHooked;
    public bool IsSubscribed { get; private set; }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (ease == null || ease.length == 0)
            ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

        shownPos = rect.anchoredPosition;
        hiddenPos = shownPos + new Vector2(0, -slideOffset);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        rect.anchoredPosition = hiddenPos;
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
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(FadeSlide(show));
    }

    private IEnumerator FadeSlide(bool show)
    {
        // for editmode tests, skip the animation and set to end state
        if (!Application.isPlaying)
        {
            float targetAlpha = show ? 1f : 0f;
            Vector2 targetPos = show ? shownPos : hiddenPos;

            canvasGroup.alpha = targetAlpha;
            rect.anchoredPosition = targetPos;

            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;

            yield break;
        }
                
        float time = 0;
        float duration = Mathf.Max(0.01f, fadeDuration);

        float startA = canvasGroup.alpha;
        float endA = show ? 1 : 0;

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = show ? shownPos : hiddenPos;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = ease.Evaluate(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startA, endA, t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return new WaitForEndOfFrame(); // this ensures that this is updated per frame
        }

        canvasGroup.alpha = endA;
        rect.anchoredPosition = endPos;

        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;
    }

    //shows Player X's Turn
    public void ShowBanner(int playerId)
    {
        if (turnLabel != null)
            turnLabel.text = $"Player {playerId}'s Turn";
        
        PlayAnim(true);
    }

    //Could be used to show turn number
    public void ShowTurn(int playerId, int turnNum)
    {
        if (turnLabel != null)
            turnLabel.text = $"Turn {turnNum} — Player {playerId}";
        
        PlayAnim(true);
    }


}
