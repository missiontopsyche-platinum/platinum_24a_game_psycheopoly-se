using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuPrototype : MonoBehaviour
{
    [SerializeField] private Material tmpMat;
    [SerializeField] private Button startButton;

    [Header("Animation Parameters")] 
    [SerializeField] private float animTime;
    [SerializeField] private AnimationCurve easeCurve;

    private bool isRunning = false;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FadeUI(fadeIn:true));
    }

    private void OnDestroy()
    {
        tmpMat.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
    }

    public void OnClick()
    {
        // blocking until fade is complete.
        if (!isRunning)
            StartCoroutine(FadeUI(fadeIn: false));
    }

    private void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    private IEnumerator FadeUI(bool fadeIn)
    {
        isRunning = true;
        
        float dilateTarget = fadeIn ? 0f : -1f;
        float dilateStart = fadeIn ? -1f : 0f;
        
        var buttonTransform = startButton.GetComponent<RectTransform>();
        float buttonStartScale = fadeIn ? 0f : buttonTransform.localScale.x;
        float buttonEndScale = fadeIn ? buttonTransform.localScale.x : 0f;
        
        buttonTransform.localScale = buttonStartScale * Vector3.one;
        tmpMat.SetFloat(ShaderUtilities.ID_FaceDilate, dilateStart);

        if (fadeIn)
            yield return new WaitForSeconds(1.5f); // wait for stuttering to end... not sure why thats happening
        
        float startTime = Time.time;
        float currentTime = startTime;
        
        while (currentTime - startTime <= animTime)
        {
            currentTime = Time.time;
            
            var linearT = (currentTime - startTime) / animTime;
            var easedT = easeCurve.Evaluate(linearT);
            
            tmpMat.SetFloat(ShaderUtilities.ID_FaceDilate, Mathf.Lerp(dilateStart, dilateTarget, easedT));
            buttonTransform.localScale = Mathf.Lerp(buttonStartScale, buttonEndScale, easedT) * Vector3.one;

            yield return new WaitForEndOfFrame();
        }
    
        if (!fadeIn)
            StartGame();
        
        isRunning = false;
    }
}
