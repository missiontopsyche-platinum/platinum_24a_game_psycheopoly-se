using TMPro;
using UnityEngine;
using UI;

public class WinScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text winnerNameText;

    [Header("Scene Flow")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    private void Start()
    {
        if (winnerNameText != null)
        {
            winnerNameText.text = string.IsNullOrWhiteSpace(GameManager.LastWinningPlayerName)
                ? "No Winner"
                : GameManager.LastWinningPlayerName;
        }
    }

    public void ReturnToMainMenu()
    {
        if (SceneTransitioner.instance != null)
        {
            SceneTransitioner.instance.TransitionScene(mainMenuSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneIndex);
        }
    }
}