using System.Collections;
using UnityEngine;
using CombatGame.Data;

namespace CombatGame.Combat
{
    /// <summary>
    /// Resolves a single AttackData: plays the attacker's animation, waits the signal delay,
    /// shows the signal, opens the input window, determines Perfect/Good/Miss,
    /// and plays the defender's reaction animation.
    /// </summary>
    public class AttackResolver : MonoBehaviour
    {
        public SignalUI signalUI;

        public IEnumerator ResolveSimple(AttackData attack, bool listenForAttackButton,
            CharacterAnimationController attacker, CharacterAnimationController defender,
            System.Action<HitResult> onResult)
        {
            attacker.PlayAttack(attack.animationTrigger);

            yield return new WaitForSeconds(attack.signalDelay);

            signalUI.Show(SignalType.Simple);

            float elapsed = 0f;
            HitResult result = HitResult.Miss;
            bool pressedDuringWindow = false;
            float pressTime = -1f;

            float perfectStart = (attack.inputWindow - attack.perfectWindow) / 2f;
            float perfectEnd = perfectStart + attack.perfectWindow;

            void HandlePress()
            {
                if (pressedDuringWindow) return;
                pressedDuringWindow = true;
                pressTime = elapsed;
            }

            if (listenForAttackButton) CombatInput.Instance.OnAttackPressed += HandlePress;
            else CombatInput.Instance.OnBlockPressed += HandlePress;

            while (elapsed < attack.inputWindow)
            {
                elapsed += Time.deltaTime;
                signalUI.SetFill(1f - (elapsed / attack.inputWindow));
                yield return null;
            }

            if (listenForAttackButton) CombatInput.Instance.OnAttackPressed -= HandlePress;
            else CombatInput.Instance.OnBlockPressed -= HandlePress;

            if (pressedDuringWindow)
            {
                result = (pressTime >= perfectStart && pressTime <= perfectEnd) ? HitResult.Perfect : HitResult.Good;
            }

            signalUI.Hide();
            PlayDefenderReaction(defender, listenForAttackButton, attack, result);
            onResult?.Invoke(result);
        }

        public IEnumerator ResolveCharged(AttackData attack, bool listenForAttackButton,
            CharacterAnimationController attacker, CharacterAnimationController defender,
            System.Action<HitResult> onResult)
        {
            attacker.PlayAttack(attack.animationTrigger);

            yield return new WaitForSeconds(attack.signalDelay);

            signalUI.Show(SignalType.Charged);

            bool isHeld() => listenForAttackButton ? CombatInput.Instance.AttackHeld : CombatInput.Instance.BlockHeld;

            float graceElapsed = 0f;
            while (!isHeld() && graceElapsed < attack.inputWindow)
            {
                graceElapsed += Time.deltaTime;
                signalUI.SetFill(1f - (graceElapsed / attack.inputWindow));
                yield return null;
            }

            if (!isHeld())
            {
                signalUI.Hide();
                PlayDefenderReaction(defender, listenForAttackButton, attack, HitResult.Miss);
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            // Only the Hero holds a physical "block pose" - enemies don't block, so only
            // set IsBlockHolding when the defender is reacting on the Block button.
            if (!listenForAttackButton) defender.SetBlockHolding(true);

            float heldElapsed = 0f;
            while (heldElapsed < attack.holdDuration)
            {
                if (!isHeld())
                {
                    if (!listenForAttackButton) defender.SetBlockHolding(false);
                    signalUI.Hide();
                    PlayDefenderReaction(defender, listenForAttackButton, attack, HitResult.Miss);
                    onResult?.Invoke(HitResult.Miss);
                    yield break;
                }
                heldElapsed += Time.deltaTime;
                signalUI.SetFill(heldElapsed / attack.holdDuration);
                yield return null;
            }

            if (!listenForAttackButton) defender.SetBlockHolding(false);
            signalUI.Hide();
            onResult?.Invoke(HitResult.Good);
        }

        /// <summary>
        /// Plays the defender's reaction animation based on who attacked and the result.
        /// - Enemy attacks, Hero defends: Perfect/Good -> Block (parry), Miss -> Hurt.
        /// - Hero attacks, Enemy defends: Perfect/Good -> Hurt (hit landed), Miss -> nothing (attack whiffed).
        /// </summary>
        private void PlayDefenderReaction(CharacterAnimationController defender, bool attackerIsHero, AttackData attack, HitResult result)
        {
            if (attackerIsHero)
            {
                // Enemy is defending against Hero's attack
                if (result != HitResult.Miss) defender.PlayHurt();
                // Miss: attack whiffed, enemy stays in Idle, no reaction needed
            }
            else
            {
                // Hero is defending against Enemy's attack
                if (result == HitResult.Miss) defender.PlayHurt();
                else defender.PlayBlock();
            }
        }
    }
}