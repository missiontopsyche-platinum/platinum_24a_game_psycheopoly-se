using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SceneTransitioner : MonoBehaviour
    {
        [SerializeField] private Image screenCover;
        
        [Header("Animation Parameters")] 
        [SerializeField] private float animTime;
        [SerializeField] private AnimationCurve easeCurve;

        private bool isRunning;
        
        public static SceneTransitioner instance { get; private set; }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
        
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // fade in at start
            StartCoroutine(WakeUp());
        }

        private IEnumerator WakeUp()
        {
            yield return new WaitForSeconds(2f); // wait for game init
            yield return StartCoroutine(Fade(1f, 0f));
        }

        public void TransitionScene(int sceneIndex)
        {
            if (isRunning) return;
            StartCoroutine(RunTransition(sceneIndex));
        }
        
        private IEnumerator RunTransition(int sceneIndex)
        {
            // fade from black
            yield return StartCoroutine(Fade(0f, 1f));
            
            yield return new WaitForEndOfFrame();
            
            SceneManager.LoadScene(sceneIndex);
            
            // if this feels weird we can remove it
            yield return new WaitForSecondsRealtime(1f);
            
            // fade to black
            yield return StartCoroutine(Fade(1f, 0f));
        }

        public void Quit()
        {
            if (isRunning) return;
            StartCoroutine(RunQuit());
        }

        private IEnumerator RunQuit()
        {
            // fade out
            yield return StartCoroutine(Fade(0f, 1f));
            
            Application.Quit();
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            isRunning = true;

            var startColor = screenCover.color;
            startColor.a = startAlpha;
            var endColor = screenCover.color;
            endColor.a = endAlpha;
            
            float startTime = Time.time;
            float currentTime = startTime;
        
            while (currentTime - startTime <= animTime)
            {
                currentTime = Time.time;
            
                var linearT = (currentTime - startTime) / animTime;
                var easedT = easeCurve.Evaluate(linearT);

                screenCover.color = Color.Lerp(startColor, endColor, easedT);

                yield return new WaitForEndOfFrame();
            }
            isRunning = false;
        }
    }
}