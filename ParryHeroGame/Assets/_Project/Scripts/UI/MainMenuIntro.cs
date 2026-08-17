using UnityEngine;
using CombatGame.Core;

namespace CombatGame.UI
{
    /// <summary>
    /// Fades in from black when the Main Menu scene loads (e.g. on game launch).
    /// </summary>
    public class MainMenuIntro : MonoBehaviour
    {
        public float fadeInDuration = 1.0f;

        private void Start()
        {
            if (SceneFader.Instance != null)
                StartCoroutine(SceneFader.Instance.FadeIn(fadeInDuration));
        }
    }
}