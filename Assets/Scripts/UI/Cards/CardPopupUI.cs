using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class CardPopupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text cardTitleText;
    [SerializeField] private Text cardBodyText;
    [SerializeField] private Button okButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image artworkImage;
    [SerializeField] private CardDrawnEventChannel cardDrawnChannel;
    [SerializeField] private float fadeDuration = 0.25f;

    private Card currentCard;
    private Player currentPlayer;
    private CardDeck currentDeck;
    public Button OkButton => okButton;

    private Coroutine fadeRoutine;

    private void OnEnable() 
    {
        if(cardDrawnChannel != null){
            cardDrawnChannel.Subscribe(OnCardDrawn);
        }
    }

    private void OnDisable()
    {
        if(cardDrawnChannel != null)
        {
            cardDrawnChannel.Unsubscribe(OnCardDrawn);
        }
    }

    //Connects the OK button to code
    private void Awake()
    {
        okButton.onClick.AddListener(OnOkClicked); 
    }

    //called when CardDeck raises the CardDrawn Event
    private void OnCardDrawn(Card card, Player player, CardDeck deck)
    {
        currentCard = card;
        currentPlayer = player;
        currentDeck = deck;

        ShowCard(card);
    }

    private void ShowCard(Card card)
    {
        if(card == null)
        {
            return; 
        }

        if(!gameObject.activeSelf)
        {
            gameObject.SetActive(true);

            //Assign the UI feilds from card Scriptable Object
            if(cardTitleText != null) 
            {
                cardTitleText.text = card.title; //use real names
            }

            if(cardBodyText != null)
            {
                cardBodyText.text = card.bodyText; //use real description
            }

            if(artworkImage != null)
            {
                artworkImage.sprite = card.artwork; //if we want to add images
            }

            //Fade in
            if(fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(FadeIn());
        }
    }

    private void OnOkClicked()
    {
        //If something went wrong just hide the card 
        if(currentCard == null || currentPlayer == null || currentDeck == null)
        {
            HideInstant();
            return;
        }

        //execute card effect 
        foreach (var effect in currentCard.effect)
        {
            effect.ApplyEffect(currentPlayer);
        }

        //return to deck 
        currentDeck.ReturnCardToDeck(currentCard);

        //Clear the states
        currentCard = null;
        currentPlayer = null;
        currentDeck = null;

        //Fade out
        StartHideAnimation();
    }

    //Helper to instantly hide without any animation 
    private void HideInstant()
    {
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    //starts the hide animiation 
    private void StartHideAnimation()
    {
        if(fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        if(canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float time = 0f;

        while(time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if(canvasGroup == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float startalpha = canvasGroup.alpha;
        float time = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while(time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startalpha, 0f, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false); 
    }

}
