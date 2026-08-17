using System.Collections;
using UnityEngine;

namespace CombatGame.Core
{
    /// <summary>
    /// Persistent full-screen black overlay used for fade-to-black / fade-from-black
    /// transitions between scenes. Access from anywhere via SceneFader.Instance.
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1.0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public IEnumerator FadeOut(float duration)
        {
            yield return Fade(0f, 1f, duration);
            canvasGroup.blocksRaycasts = true;
        }

        public IEnumerator FadeIn(float duration)
        {
            canvasGroup.blocksRaycasts = false;
            yield return Fade(1f, 0f, duration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            canvasGroup.alpha = from;
            while (t < duration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}