using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal.Filters;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuPrototype : MonoBehaviour
{
    [SerializeField] private Material tmpMat;
    [SerializeField] private List<Button> buttons;
    [SerializeField] private Image logo;

    [Header("Animation Parameters")] 
    [SerializeField] private float animTime;
    [SerializeField] private AnimationCurve easeCurve;

    private bool isRunning = false;
    private float buttonScaleFactor;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonScaleFactor = buttons[0].transform.localScale.x;
        StartCoroutine(FadeUI(fadeIn:true));
    }

    private void OnDestroy()
    {
        tmpMat.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
    }

    public void FadeOut()
    {
        // blocking until fade is complete.
        if (!isRunning)
            StartCoroutine(FadeUI(fadeIn: false));
    }

    public void FadeIn()
    {
        if (!isRunning)
            StartCoroutine(FadeUI(fadeIn: true));
    }

    private IEnumerator FadeUI(bool fadeIn)
    {
        isRunning = true;
        buttons.ForEach(b => b.interactable = false);
        
        float dilateTarget = fadeIn ? 0f : -1f;
        float dilateStart = fadeIn ? -1f : 0f;
        
        float buttonStartScale = fadeIn ? 0f : buttonScaleFactor;
        float buttonEndScale = fadeIn ? buttonScaleFactor : 0f;

        var logoColorStart = fadeIn ? Color.clear : Color.white;
        var logoColorEnd = fadeIn ? Color.white : Color.clear;
        
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
            
            logo.color = Color.Lerp(logoColorStart, logoColorEnd, easedT);
            tmpMat.SetFloat(ShaderUtilities.ID_FaceDilate, Mathf.Lerp(dilateStart, dilateTarget, easedT));

            var scaleT = Mathf.Lerp(buttonStartScale, buttonEndScale, easedT) * Vector3.one;
            buttons.ForEach(b => b.transform.localScale = scaleT);

            yield return new WaitForEndOfFrame();
        }
        
        isRunning = false;
        buttons.ForEach(b => b.interactable = true);
    }
}
