using UnityEngine;

namespace CombatGame.Combat
{
    /// <summary>
    /// Keeps the Hero's block pose synced to raw input at all times, independent of
    /// whether there's an active parry window. This gives instant visual feedback
    /// (tap = quick raise/lower, hold = sustained pose) regardless of combat resolution.
    /// The Animator itself decides tap vs hold based on how long IsBlockHolding stays true -
    /// no extra logic needed here.
    /// </summary>
    [RequireComponent(typeof(CharacterAnimationController))]
    public class HeroBlockReactor : MonoBehaviour
    {
        private CharacterAnimationController animController;

        private void Awake()
        {
            animController = GetComponent<CharacterAnimationController>();
        }

        private void Update()
        {
            if (CombatInput.Instance == null) return;
            animController.SetBlockHolding(CombatInput.Instance.BlockHeld);
        }
    }
}   