using UnityEngine;
using UnityEngine.InputSystem;

namespace CombatGame.Combat
{
    /// <summary>
    /// Centralizes player input so the rest of the combat system doesn't care
    /// whether it comes from keyboard (PC) or on-screen buttons (mobile).
    /// Uses the new Input System package.
    /// Mobile buttons should call PressBlock/ReleaseBlock and PressAttack/ReleaseAttack
    /// via UI EventTrigger (PointerDown/PointerUp), same as this class does for keys.
    /// </summary>
    public class CombatInput : MonoBehaviour
    {
        public static CombatInput Instance { get; private set; }

        public bool BlockHeld { get; private set; }
        public bool AttackHeld { get; private set; }

        public event System.Action OnBlockPressed;
        public event System.Action OnBlockReleased;
        public event System.Action OnAttackPressed;
        public event System.Action OnAttackReleased;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // PC keys: Q = Block/Parry, E = Attack
            if (Keyboard.current.qKey.wasPressedThisFrame) PressBlock();
            if (Keyboard.current.qKey.wasReleasedThisFrame) ReleaseBlock();

            if (Keyboard.current.eKey.wasPressedThisFrame) PressAttack();
            if (Keyboard.current.eKey.wasReleasedThisFrame) ReleaseAttack();
        }

        public void PressBlock()
        {
            if (BlockHeld) return;
            BlockHeld = true;
            OnBlockPressed?.Invoke();
        }

        public void ReleaseBlock()
        {
            if (!BlockHeld) return;
            BlockHeld = false;
            OnBlockReleased?.Invoke();
        }

        public void PressAttack()
        {
            if (AttackHeld) return;
            AttackHeld = true;
            OnAttackPressed?.Invoke();
        }

        public void ReleaseAttack()
        {
            if (!AttackHeld) return;
            AttackHeld = false;
            OnAttackReleased?.Invoke();
        }
    }
}