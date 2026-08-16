using UnityEngine;
using System.Collections;

namespace CombatGame.Core
{
    /// <summary>
    /// Persistent, scene-independent audio hub. Handles one-shot SFX and looping music.
    /// Access from anywhere via AudioManager.Instance.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volumes")]
        [Range(0f, 1f)] public float musicVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 1f;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
                yield return null;
            }
            musicSource.volume = 0f;
            musicSource.Stop();
            musicSource.volume = musicVolume; // reset for next PlayMusic call
        }

        public IEnumerator FadeInMusic(AudioClip clip, float duration)
        {
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t / duration);
                yield return null;
            }
            musicSource.volume = musicVolume;
        }

    }
}