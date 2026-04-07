using TMPro;
using UnityEngine;
using UI;

public class WinScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text winnerText;

    [Header("Scene Flow")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    private void Start()
    {
        if (winnerText != null)
        {
            string winnerName = string.IsNullOrWhiteSpace(GameManager.LastWinningPlayerName)
                ? "No Winner"
                : GameManager.LastWinningPlayerName;

            winnerText.text = $"Winner: {winnerName}";
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