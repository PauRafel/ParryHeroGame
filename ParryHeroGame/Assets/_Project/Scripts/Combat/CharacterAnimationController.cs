using UnityEngine;

namespace CombatGame.Combat
{
    /// <summary>
    /// Bridges combat logic with the Animator. Works for both Hero and Enemy
    /// as long as their Animator Controllers share the same parameter names.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        private Animator animator;

        private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
        private static readonly int DeathTrigger = Animator.StringToHash("Death");
        private static readonly int BlockTrigger = Animator.StringToHash("Block");
        private static readonly int IsBlockHolding = Animator.StringToHash("IsBlockHolding");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Plays an attack by its trigger name, e.g. "Attack1", "Attack2", "Attack3".
        /// Trigger name comes directly from AttackData.animationTrigger.
        /// </summary>
        public void PlayAttack(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName)) return;
            animator.SetTrigger(triggerName);
        }

        public void PlayBlock()
        {
            animator.SetTrigger(BlockTrigger);
        }

        public void SetBlockHolding(bool holding)
        {
            animator.SetBool(IsBlockHolding, holding);
        }

        public void PlayHurt()
        {
            animator.SetTrigger(HurtTrigger);
        }

        public void PlayDeath()
        {
            animator.SetTrigger(DeathTrigger);
        }
    }
}