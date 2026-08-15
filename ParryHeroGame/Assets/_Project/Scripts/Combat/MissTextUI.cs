using System.Collections;
using UnityEngine;

namespace CombatGame.Combat
{
    /// <summary>
    /// Shows a brief "MISS" popup as feedback when the Hero's attack fails.
    /// </summary>
    public class MissTextUI : MonoBehaviour
    {
        public GameObject root;
        public float displayDuration = 0.6f;

        private Coroutine activeRoutine;

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        public void ShowMiss()
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            activeRoutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (root != null) root.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            if (root != null) root.SetActive(false);
        }
    }
}