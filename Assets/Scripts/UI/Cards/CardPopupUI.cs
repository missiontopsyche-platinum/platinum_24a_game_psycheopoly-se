using UnityEngine;
using UnityEngine.UI; 

public class CardPopupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text cardTitleText;
    [SerializeField] private Text cardBodyText;
    [SerializeField] private Button okButton;
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private Image artworkImage;
    [SerializeField] private CardDrawnEventChannel cardDrawnChannel;
    private Card currentCard;
    private Player currentPlayer;
    private CardDeck currentDeck;

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

    private void OnCardDrawn(Card card, Player player, CardDeck deck)
    {
        currentCard = card;
        currentPlayer = player;
        currentDeck = deck;

        //ShowCard(card);
    }
}
