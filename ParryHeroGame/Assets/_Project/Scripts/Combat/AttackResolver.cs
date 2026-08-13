using System.Collections;
using UnityEngine;
using CombatGame.Data;

namespace CombatGame.Combat
{
    /// <summary>
    /// Resolves a single AttackData: waits the signal delay, shows the signal,
    /// opens the input window, and determines Perfect/Good/Miss.
    /// </summary>
    public class AttackResolver : MonoBehaviour
    {
        public SignalUI signalUI;

        public IEnumerator ResolveSimple(AttackData attack, bool listenForAttackButton, System.Action<HitResult> onResult)
        {
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
                // 1 = full time left, 0 = window closed
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
            onResult?.Invoke(result);
        }

        public IEnumerator ResolveCharged(AttackData attack, bool listenForAttackButton, System.Action<HitResult> onResult)
        {
            yield return new WaitForSeconds(attack.signalDelay);

            signalUI.Show(SignalType.Charged);

            bool isHeld() => listenForAttackButton ? CombatInput.Instance.AttackHeld : CombatInput.Instance.BlockHeld;

            // Grace period to start holding
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
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            // Now show hold progress filling up (0 -> 1) instead of counting down
            float heldElapsed = 0f;
            while (heldElapsed < attack.holdDuration)
            {
                if (!isHeld())
                {
                    signalUI.Hide();
                    onResult?.Invoke(HitResult.Miss);
                    yield break;
                }
                heldElapsed += Time.deltaTime;
                signalUI.SetFill(heldElapsed / attack.holdDuration);
                yield return null;
            }

            signalUI.Hide();
            onResult?.Invoke(HitResult.Good);
        }
    }
}