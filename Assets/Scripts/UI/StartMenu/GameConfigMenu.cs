using UnityEngine;
using UnityEngine.SceneManagement;

public class GameConfigMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
