using UnityEngine;
using UnityEngine.UI;


//created for US517 in making sure we have a setup for the card view UI
public class CardView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text cardTitleText;
    [SerializeField] private Text cardBodyText;
    [SerializeField] private Image artworkImage;

    private Card currentCard;

    // Called by CardPopupUI (or anything else) to display a card
    public void SetCard(Card card)
    {
        if (card == null)
        {
            Clear();
            return;
        }

        currentCard = card;

        if (cardTitleText != null)
        {
            cardTitleText.text = card.title;
        }

        if (cardBodyText != null)
        {
            cardBodyText.text = card.bodyText;

        }

        //this will come next
        if (artworkImage != null)
        {
            artworkImage.sprite = card.artwork;
        }
    }


    public void Clear()
    {
        currentCard = null;

        if (cardTitleText != null)
        {
            cardTitleText.text = "";
        }

        if (cardBodyText != null)
        {
            cardBodyText.text = "";
        }

        if (artworkImage != null)
        {
            artworkImage.sprite = null;
        }
    }
}
