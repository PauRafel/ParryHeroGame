using UnityEngine;
using UnityEngine.UI;
using CombatGame.Data;

namespace CombatGame.Combat
{
    /// <summary>
    /// Displays the timing signal: a colored icon (yellow = Simple, red = Charged)
    /// with a radial fill representing time remaining / hold progress.
    /// Placeholder art: plain colored circles, swap sprites later without touching logic.
    /// </summary>
    public class SignalUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject root;          // parent object to show/hide the whole signal
        public Image icon;               // center icon, color-tinted
        public Image radialFill;         // Image Type = Filled, Fill Method = Radial 360

        [Header("Colors")]
        public Color simpleColor = Color.yellow;
        public Color chargedColor = new Color(0.8f, 0.1f, 0.1f); // crimson

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        public void Show(SignalType type)
        {
            if (root != null) root.SetActive(true);
            Color c = type == SignalType.Simple ? simpleColor : chargedColor;
            if (icon != null) icon.color = c;
            if (radialFill != null)
            {
                radialFill.color = c;
                radialFill.fillAmount = 1f;
            }
        }

        /// <summary>
        /// 1 = full time remaining / no hold progress, 0 = time's up / hold complete.
        /// Caller decides the meaning (countdown vs progress) by how it drives this value.
        /// </summary>
        public void SetFill(float value01)
        {
            if (radialFill != null) radialFill.fillAmount = Mathf.Clamp01(value01);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}