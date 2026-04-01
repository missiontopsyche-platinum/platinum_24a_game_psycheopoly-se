using Data;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameConfigMenu : MonoBehaviour
{
    [SerializeField] private PlayerSetupController playerSetupController;
    
    public void StartGame()
    {
        // load player configs into static config list
        GameConfiguration.playerConfigs = playerSetupController.GetPlayerConfigs();
        
        // when we add more config options we can add them to the GameConfiguration here
        
        // fade to black and load new scene
        SceneTransitioner.instance.TransitionScene(1);
    }
}
