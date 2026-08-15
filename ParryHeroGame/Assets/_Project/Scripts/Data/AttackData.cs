using UnityEngine;

namespace CombatGame.Data
{
    /// <summary>
    /// Represents a single hit within a combo (either an enemy attack the player
    /// must Parry/Block, or a Hero attack the player must land).
    /// One AttackData = one signal shown to the player.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttackData", menuName = "CombatGame/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("Signal")]
        public SignalType signalType = SignalType.Simple;

        [Header("Timing (seconds)")]
        [Tooltip("Delay from the start of the wind-up animation until the signal appears.")]
        public float signalDelay = 0.5f;

        [Tooltip("Simple: total window the player has to press the button after the signal appears.")]
        public float inputWindow = 0.4f;

        [Tooltip("Perfect-hit window in seconds, centered inside inputWindow. Used for scoring/ranking.")]
        public float perfectWindow = 0.1f;

        [Tooltip("Charged only: how long the player must hold the button once the signal appears.")]
        public float holdDuration = 1.0f;

        [Header("Damage")]
        [Tooltip("Damage dealt to whoever fails to react correctly to this attack.")]
        public int damageOnFail = 1;

        [Header("Pacing")]
        [Tooltip("Pause (seconds) after this attack resolves, before the next attack in the combo begins.")]
        public float pauseAfter = 1.0f;

        [Header("Animation")]
        [Tooltip("Trigger/state name in the Animator for this specific attack.")]
        public string animationTrigger;
    }
}