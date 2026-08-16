using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CombatGame.Core;

namespace CombatGame.UI
{
    /// <summary>
    /// Adds a hover feedback (slight scale-up + brighter tint) and a hover sound
    /// to any UI Button. Works purely by code, no separate hover sprite needed.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Scale")]
        public float hoverScale = 1.08f;
        public float scaleSpeed = 12f;

        [Header("Brightness")]
        [Tooltip("Multiplier applied to the button's normal color on hover (e.g. 1.3 = 30% brighter).")]
        public float brightnessMultiplier = 1.3f;

        [Header("Sound")]
        public AudioClip hoverSound;

        private Image image;
        private Color normalColor;
        private Color hoverColor;
        private Vector3 normalScale;
        private Vector3 targetScale;
        private Color targetColor;

        private void Awake()
        {
            image = GetComponent<Image>();
            normalColor = image.color;
            hoverColor = normalColor * brightnessMultiplier;
            hoverColor.a = normalColor.a; // don't multiply alpha

            normalScale = transform.localScale;
            targetScale = normalScale;
            targetColor = normalColor;
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            image.color = Color.Lerp(image.color, targetColor, Time.deltaTime * scaleSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = normalScale * hoverScale;
            targetColor = hoverColor;

            if (hoverSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hoverSound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = normalScale;
            targetColor = normalColor;
        }
    }
}