using UnityEngine;

namespace CombatGame.Data
{
    /// <summary>
    /// An ordered sequence of AttackData that makes up one full turn
    /// (e.g. Enemy's combo of 3 hits, or Hero's combo of 2 hits).
    /// </summary>
    [CreateAssetMenu(fileName = "NewComboData", menuName = "CombatGame/Combo Data")]
    public class ComboData : ScriptableObject
    {
        public TurnOwner owner = TurnOwner.Enemy;
        public AttackData[] attacks;

        [Tooltip("Pause after the last attack before the turn officially ends (lets Charged holds release cleanly).")]
        public float endOfComboPause = 2.0f;
    }
}