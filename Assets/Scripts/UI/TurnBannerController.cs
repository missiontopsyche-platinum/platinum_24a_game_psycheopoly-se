using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnBannerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text turnLabel;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        //makes sure the banner is hidden when game starts
        gameObject.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    //Called by GameManager or PlayerManager
    public void ShowBanner(int playerId)
    {
        turnLabel.text = $"Player {playerId}'s Turn";
        gameObject.SetActive(true);
    }

    private void OnContinueClicked()
    {
        //Hide the banner and tell game to continue
        gameObject.SetActive(false);

        //Example would be to notifiy GameManager that the player
        //Has started their turn, this is currently a placeholder for 
        //logic once more of the game is complete, but could look like
        //GameManager.Instance.StartTurn();
    }

}
