using System.Collections;
using UnityEngine;
using CombatGame.Data;

namespace CombatGame.Combat
{
    /// <summary>
    /// Resolves a single AttackData: plays the attacker's animation, waits the signal delay,
    /// shows the signal, opens the input window, and determines Perfect/Good/Miss.
    /// The defender's base pose (e.g. Hero's BlockIdle) is driven independently in real time
    /// by HeroBlockReactor - this class only overrides with Block (parry) or Hurt when relevant.
    /// </summary>
    public class AttackResolver : MonoBehaviour
    {
        public SignalUI signalUI;

        [Header("Hero Attack Feedback")]
        public MissTextUI missTextUI;

        private static readonly string[] SimpleAttackTriggers = { "Attack1", "Attack2", "Attack3" };
        private string RandomAttackTrigger() => SimpleAttackTriggers[Random.Range(0, SimpleAttackTriggers.Length)];

        /// <summary>
        /// Hero's Simple attack: any press during the whole resolution counts as a real attempt.
        /// Early press (before window opens) or late/timeout = Miss. Press within window = hit.
        /// </summary>
        public IEnumerator ResolveHeroAttackSimple(AttackData attack, CharacterAnimationController hero,
            CharacterAnimationController enemy, System.Action<HitResult> onResult)
        {
            bool earlyPressed = false;
            void HandleEarly() => earlyPressed = true;
            CombatInput.Instance.OnAttackPressed += HandleEarly;

            float delayElapsed = 0f;
            while (delayElapsed < attack.signalDelay && !earlyPressed)
            {
                delayElapsed += Time.deltaTime;
                yield return null;
            }
            CombatInput.Instance.OnAttackPressed -= HandleEarly;

            if (earlyPressed)
            {
                yield return HandleMiss(hero, playSwing: true);
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            signalUI.Show(SignalType.Simple);

            float elapsed = 0f;
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
            CombatInput.Instance.OnAttackPressed += HandlePress;

            while (elapsed < attack.inputWindow && !pressedDuringWindow)
            {
                elapsed += Time.deltaTime;
                signalUI.SetFill(1f - (elapsed / attack.inputWindow));
                yield return null;
            }
            CombatInput.Instance.OnAttackPressed -= HandlePress;
            signalUI.Hide();

            if (!pressedDuringWindow)
            {
                yield return HandleMiss(hero, playSwing: false);
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            HitResult result = (pressTime >= perfectStart && pressTime <= perfectEnd) ? HitResult.Perfect : HitResult.Good;

            yield return hero.PlayAttackAndWait(RandomAttackTrigger());
            enemy.PlayHurt();
            onResult?.Invoke(result);
        }

        /// <summary>
        /// Hero's Charged combo attack: must start holding exactly when the window opens
        /// (no early buffering) and sustain through Attack1 -> Attack2 -> Attack3.
        /// Releasing early at any point cuts the sequence immediately.
        /// </summary>
        public IEnumerator ResolveHeroAttackCombo(AttackData attack, CharacterAnimationController hero,
            CharacterAnimationController enemy, System.Action<HitResult> onResult)
        {
            bool isHeld() => CombatInput.Instance.AttackHeld;

            float delayElapsed = 0f;
            while (delayElapsed < attack.signalDelay && !isHeld())
            {
                delayElapsed += Time.deltaTime;
                yield return null;
            }

            if (isHeld())
            {
                // Pressed before the window even opened - invalid attempt.
                yield return HandleMiss(hero, playSwing: false);
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            signalUI.Show(SignalType.Charged);

            float graceElapsed = 0f;
            while (!isHeld() && graceElapsed < attack.inputWindow)
            {
                graceElapsed += Time.deltaTime;
                signalUI.SetFill(1f - (graceElapsed / attack.inputWindow));
                yield return null;
            }
            signalUI.Hide();

            if (!isHeld())
            {
                yield return HandleMiss(hero, playSwing: false);
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            bool interrupted = false;
            void MarkInterrupted() => interrupted = true;

            string[] comboSequence = { "Attack1", "Attack2", "Attack3" };
            foreach (string trigger in comboSequence)
            {
                yield return hero.PlayAttackWhileHeld(trigger, isHeld, MarkInterrupted);
                if (interrupted)
                {
                    hero.PlayMissFlash();
                    missTextUI?.ShowMiss();
                    yield return new WaitForSeconds(0.3f);
                    onResult?.Invoke(HitResult.Miss);
                    yield break;
                }
            }

            enemy.PlayHurt();
            onResult?.Invoke(HitResult.Good);
        }

        private IEnumerator HandleMiss(CharacterAnimationController hero, bool playSwing)
        {
            if (playSwing)
            {
                yield return hero.PlayAttackAndWait(RandomAttackTrigger());
            }
            hero.PlayMissFlash();
            missTextUI?.ShowMiss();
            yield return new WaitForSeconds(0.3f);
        }

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
            PlayDefenderReaction(defender, listenForAttackButton, result);
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
                PlayDefenderReaction(defender, listenForAttackButton, HitResult.Miss);
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            float heldElapsed = 0f;
            while (heldElapsed < attack.holdDuration)
            {
                if (!isHeld())
                {
                    signalUI.Hide();
                    PlayDefenderReaction(defender, listenForAttackButton, HitResult.Miss);
                    onResult?.Invoke(HitResult.Miss);
                    yield break;
                }
                heldElapsed += Time.deltaTime;
                signalUI.SetFill(heldElapsed / attack.holdDuration);
                yield return null;
            }

            signalUI.Hide();
            // Charged success: no animation override needed, BlockIdle is already
            // playing naturally because the player is holding the button (driven by HeroBlockReactor).
            onResult?.Invoke(HitResult.Good);
        }

        /// <summary>
        /// Overrides the defender's free-response pose when the timing result matters:
        /// - Enemy attacks, Hero defends: Miss -> Hurt. Perfect/Good (Simple only) -> Block (parry w/ effect).
        /// - Hero attacks, Enemy defends: Perfect/Good -> Hurt (hit landed). Miss -> nothing (whiffed).
        /// </summary>
        private void PlayDefenderReaction(CharacterAnimationController defender, bool attackerIsHero, HitResult result)
        {
            if (attackerIsHero)
            {
                if (result != HitResult.Miss) defender.PlayHurt();
            }
            else
            {
                if (result == HitResult.Miss) defender.PlayHurt();
                else defender.PlayBlock();
            }
        }
    }
}