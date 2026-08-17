using UnityEngine;

namespace CombatGame.UI
{
    /// <summary>
    /// Makes a UI element gently bob up and down in a continuous loop,
    /// e.g. for a menu title that shouldn't feel static.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TitleBobber : MonoBehaviour
    {
        [Tooltip("How far (pixels) the title moves up/down from its resting position.")]
        public float bobAmplitude = 8f;

        [Tooltip("How fast the bob cycle repeats. Higher = faster.")]
        public float bobSpeed = 1.5f;

        private RectTransform rect;
        private Vector2 basePosition;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            basePosition = rect.anchoredPosition;
        }

        private void Update()
        {
            float offsetY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            rect.anchoredPosition = basePosition + new Vector2(0f, offsetY);
        }
    }
}