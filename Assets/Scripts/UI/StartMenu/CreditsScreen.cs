using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.StartMenu
{
    public class CreditsScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform creditsPanel;
        [SerializeField] private List<Button> buttons;

        [Header("Animation Parameters")] 
        [SerializeField] private Vector2 hidePosition;
        [SerializeField] private float animTime;
        [SerializeField] private AnimationCurve easeCurve;

        private void Awake()
        {
            creditsPanel.anchoredPosition = hidePosition;
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
            
            var posStart = creditsPanel.anchoredPosition;
            var posEnd = isShow ? Vector2.zero : hidePosition;

            float startTime = Time.time;
            float currentTime = startTime;

            while (currentTime - startTime <= animTime)
            {
                currentTime = Time.time;

                var linearT = (currentTime - startTime) / animTime;
                var easedT = easeCurve.Evaluate(linearT);

                creditsPanel.anchoredPosition = Vector2.Lerp(posStart, posEnd, easedT);

                yield return new WaitForEndOfFrame();
            }
            
            buttons.ForEach(b => b.interactable = true);
        }
    }
}