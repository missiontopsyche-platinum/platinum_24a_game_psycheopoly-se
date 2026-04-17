using System.Collections;
using System.Collections.Generic;
using Data;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class GameConfigMenu : MonoBehaviour
{
    [SerializeField] private PlayerSetupController playerSetupController;
    [SerializeField] private RectTransform configPanel;
    [SerializeField] private List<Button> buttons;

    [Header("Animation Parameters")] 
    [SerializeField] private Vector2 hidePosition;
    [SerializeField] private float animTime;
    [SerializeField] private AnimationCurve easeCurve;

    private void Awake()
    {
        configPanel.anchoredPosition = hidePosition;
    }

    public void Show()
    {
        StartCoroutine(Slide(isShow: true));
    }

    public void Hide()
    {
        StartCoroutine(Slide(isShow: false));
    }

    private IEnumerator Slide(bool isShow)
    {
        buttons.ForEach(b => b.interactable = false);
        
        var posStart = configPanel.anchoredPosition;
        var posEnd = isShow ? Vector2.zero : hidePosition;

        float startTime = Time.time;
        float currentTime = startTime;

        while (currentTime - startTime <= animTime)
        {
            currentTime = Time.time;

            var linearT = (currentTime - startTime) / animTime;
            var easedT = easeCurve.Evaluate(linearT);

            configPanel.anchoredPosition = Vector2.Lerp(posStart, posEnd, easedT);

            yield return new WaitForEndOfFrame();
        }
        
        buttons.ForEach(b => b.interactable = true);
    }
    
    public void StartGame()
    {
        // load player configs into static config list
        GameConfiguration.playerConfigs = playerSetupController.GetPlayerConfigs();
        
        // when we add more config options we can add them to the GameConfiguration here
        
        // fade to black and load new scene
        SceneTransitioner.instance.TransitionScene(1);
    }
}
