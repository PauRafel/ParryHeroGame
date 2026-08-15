using UnityEngine;

namespace CombatGame.Combat
{
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimationController : MonoBehaviour
    {
        private Animator animator;

        private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
        private static readonly int DeathTrigger = Animator.StringToHash("Death");
        private static readonly int BlockTrigger = Animator.StringToHash("Block");
        private static readonly int IsBlockHolding = Animator.StringToHash("IsBlockHolding");

        // Attack triggers are set by name (from AttackData.animationTrigger), so we track
        // hashes we've seen to be able to reset them too.
        private static readonly string[] KnownAttackTriggerNames = { "Attack1", "Attack2", "Attack3" };

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

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
            // Clear any other trigger that might still be armed (e.g. Hurt from the
            // killing blow) so it can't fire from Any State right after Death plays.
            animator.ResetTrigger(HurtTrigger);
            animator.ResetTrigger(BlockTrigger);
            foreach (string attackTrigger in KnownAttackTriggerNames)
            {
                animator.ResetTrigger(attackTrigger);
            }

            animator.SetTrigger(DeathTrigger);
        }
    }
}