using System.Collections;
using UnityEngine;
using CombatGame.Data;

namespace CombatGame.Combat
{
    /// <summary>
    /// Resolves a single AttackData: waits the signal delay, "shows" the signal,
    /// opens the input window, and determines Perfect/Good/Miss.
    /// Works the same whether the owner is Enemy (player must Block) or Hero (player must Attack) -
    /// the caller decides which button to listen to.
    /// </summary>
    public class AttackResolver : MonoBehaviour
    {
        public IEnumerator ResolveSimple(AttackData attack, bool listenForAttackButton, System.Action<HitResult> onResult)
        {
            // Wind-up delay before the signal appears
            yield return new WaitForSeconds(attack.signalDelay);

            Debug.Log($"[Signal] SIMPLE appeared - window {attack.inputWindow}s");

            float elapsed = 0f;
            HitResult result = HitResult.Miss;
            bool pressedDuringWindow = false;
            float pressTime = -1f;

            // Perfect window is centered inside the input window
            float perfectStart = (attack.inputWindow - attack.perfectWindow) / 2f;
            float perfectEnd = perfectStart + attack.perfectWindow;

            void HandlePress()
            {
                if (pressedDuringWindow) return; // ignore extra presses
                pressedDuringWindow = true;
                pressTime = elapsed;
            }

            if (listenForAttackButton) CombatInput.Instance.OnAttackPressed += HandlePress;
            else CombatInput.Instance.OnBlockPressed += HandlePress;

            while (elapsed < attack.inputWindow)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (listenForAttackButton) CombatInput.Instance.OnAttackPressed -= HandlePress;
            else CombatInput.Instance.OnBlockPressed -= HandlePress;

            if (pressedDuringWindow)
            {
                result = (pressTime >= perfectStart && pressTime <= perfectEnd) ? HitResult.Perfect : HitResult.Good;
            }

            Debug.Log($"[Result] SIMPLE -> {result}");
            onResult?.Invoke(result);
        }

        public IEnumerator ResolveCharged(AttackData attack, bool listenForAttackButton, System.Action<HitResult> onResult)
        {
            yield return new WaitForSeconds(attack.signalDelay);

            Debug.Log($"[Signal] CHARGED appeared - must hold for {attack.holdDuration}s");

            bool isHeld() => listenForAttackButton ? CombatInput.Instance.AttackHeld : CombatInput.Instance.BlockHeld;

            // Grace period to start pressing (reuse inputWindow as "time allowed to start holding")
            float graceElapsed = 0f;
            while (!isHeld() && graceElapsed < attack.inputWindow)
            {
                graceElapsed += Time.deltaTime;
                yield return null;
            }

            if (!isHeld())
            {
                Debug.Log("[Result] CHARGED -> Miss (never pressed)");
                onResult?.Invoke(HitResult.Miss);
                yield break;
            }

            // Must keep holding for the full duration
            float heldElapsed = 0f;
            while (heldElapsed < attack.holdDuration)
            {
                if (!isHeld())
                {
                    Debug.Log("[Result] CHARGED -> Miss (released early)");
                    onResult?.Invoke(HitResult.Miss);
                    yield break;
                }
                heldElapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log("[Result] CHARGED -> Good");
            onResult?.Invoke(HitResult.Good);
        }
    }
}