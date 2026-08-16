using System.Collections;
using UnityEngine;

namespace CombatGame.Core
{
    /// <summary>
    /// Handles the fade-in from black and starts combat music when the Combat scene loads.
    /// </summary>
    public class CombatSceneIntro : MonoBehaviour
    {
        public AudioClip combatMusic;
        public float fadeInDuration = 2.0f;
        public float musicFadeInDuration = 1.0f;

        private void Start()
        {
            if (SceneFader.Instance != null)
                StartCoroutine(SceneFader.Instance.FadeIn(fadeInDuration));

            if (AudioManager.Instance != null && combatMusic != null)
                StartCoroutine(AudioManager.Instance.FadeInMusic(combatMusic, musicFadeInDuration));
        }
    }
}