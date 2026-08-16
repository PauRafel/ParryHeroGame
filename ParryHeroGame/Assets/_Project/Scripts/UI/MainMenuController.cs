using UnityEngine;
using UnityEngine.SceneManagement;
using CombatGame.Core;
using System.Collections;

namespace CombatGame.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene")]
        public string combatSceneName = "Combat";

        [Header("Panels")]
        public GameObject settingsPanel;
        public GameObject creditsPanel;

        [Header("Music")]
        public AudioClip menuMusic;

        [Header("Click Sound")]
        public AudioClip clickSound;

        [Header("Play Transition")]
        public AudioClip playWhooshSound;
        public float fadeOutDuration = 2.0f;
        public float musicFadeOutDuration = 1.0f;

        private void Start()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(menuMusic);
        }

        private IEnumerator TransitionToCombat()
        {
            if (AudioManager.Instance != null)
            {
                if (playWhooshSound != null) AudioManager.Instance.PlaySFX(playWhooshSound);
                StartCoroutine(AudioManager.Instance.FadeOutMusic(musicFadeOutDuration));
            }

            if (SceneFader.Instance != null)
                yield return StartCoroutine(SceneFader.Instance.FadeOut(fadeOutDuration));

            SceneManager.LoadScene(combatSceneName);
        }

        private void PlayClick()
        {
            if (clickSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(clickSound);
        }

        public void OnPlayPressed()
        {
            PlayClick();
            StartCoroutine(TransitionToCombat());
        }

        public void OnSettingsPressed()
        {
            PlayClick();
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnCreditsPressed()
        {
            PlayClick();
            if (creditsPanel != null) creditsPanel.SetActive(true);
        }

        public void OnQuitPressed()
        {
            PlayClick();
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // Called from a "back" button inside each panel
        public void ClosePanel(GameObject panel)
        {
            PlayClick();
            if (panel != null) panel.SetActive(false);
        }
    }
}