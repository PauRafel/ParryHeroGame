using System.Collections;
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
        private static readonly int MissTrigger = Animator.StringToHash("Miss");
        private static readonly int IsBlockHolding = Animator.StringToHash("IsBlockHolding");

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

        public void PlayMissFlash()
        {
            // Clear attack/block triggers so nothing queued fires right after the flash.
            foreach (string attackTrigger in KnownAttackTriggerNames) animator.ResetTrigger(attackTrigger);
            animator.ResetTrigger(BlockTrigger);
            animator.SetTrigger(MissTrigger);
        }

        public void PlayDeath()
        {
            animator.ResetTrigger(HurtTrigger);
            animator.ResetTrigger(BlockTrigger);
            animator.ResetTrigger(MissTrigger);
            foreach (string attackTrigger in KnownAttackTriggerNames) animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(DeathTrigger);
        }

        /// <summary>
        /// Plays an attack trigger and waits for its actual clip length to elapse.
        /// Used to chain combo hits (Attack1 -> Attack2 -> Attack3) without hardcoding durations.
        /// </summary>
        public IEnumerator PlayAttackAndWait(string triggerName)
        {
            animator.SetTrigger(triggerName);
            yield return null; // let the Animator process the transition
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(triggerName));
            float length = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(length);
        }

        /// <summary>
        /// Same as PlayAttackAndWait, but aborts early (invoking onInterrupted) if isHeld()
        /// becomes false before the clip finishes. Used for the charged combo sequence.
        /// </summary>
        public IEnumerator PlayAttackWhileHeld(string triggerName, System.Func<bool> isHeld, System.Action onInterrupted)
        {
            animator.SetTrigger(triggerName);
            yield return null;
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(triggerName));
            float length = animator.GetCurrentAnimatorStateInfo(0).length;

            float t = 0f;
            while (t < length)
            {
                if (!isHeld())
                {
                    onInterrupted?.Invoke();
                    yield break;
                }
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}