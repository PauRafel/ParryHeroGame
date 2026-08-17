using UnityEngine;

namespace CombatGame.UI
{
    /// <summary>
    /// Subtly drifts a UI Image within a bounded range, giving the impression of a
    /// slow, wandering camera pan without ever exposing empty space at the edges.
    /// The image's RectTransform must be larger than the visible canvas area -
    /// maxOffset should stay within that overflow margin.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BackgroundDrifter : MonoBehaviour
    {
        [Header("Drift Range")]
        [Tooltip("Maximum distance (pixels) the image can move from its centered position, on each axis.")]
        public Vector2 maxOffset = new Vector2(120f, 60f);

        [Header("Timing")]
        [Tooltip("How long (seconds) it takes to drift from one random point to the next.")]
        public float driftDuration = 6f;

        private RectTransform rect;
        private Vector2 basePosition;
        private Vector2 startPos;
        private Vector2 targetPos;
        private float t;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            basePosition = rect.anchoredPosition;
            startPos = basePosition;
            PickNewTarget();
        }

        private void Update()
        {
            t += Time.deltaTime / driftDuration;
            if (t >= 1f)
            {
                t = 0f;
                startPos = targetPos;
                PickNewTarget();
            }

            // Smoothstep for a gentle ease in/out rather than linear, constant-speed drift.
            float smoothT = t * t * (3f - 2f * t);
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
        }

        private void PickNewTarget()
        {
            float x = Random.Range(-maxOffset.x, maxOffset.x);
            float y = Random.Range(-maxOffset.y, maxOffset.y);
            targetPos = basePosition + new Vector2(x, y);
        }
    }
}