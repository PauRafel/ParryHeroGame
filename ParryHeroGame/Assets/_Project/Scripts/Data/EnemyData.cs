using UnityEngine;

namespace CombatGame.Data
{
    /// <summary>
    /// Defines a single enemy: stats + the pool of combos it can use.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "CombatGame/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName;
        public Sprite portrait;

        [Header("Stats")]
        public int maxHealth = 10;

        [Header("Combos")]
        [Tooltip("Pool of possible combos this enemy can throw. One is picked (randomly, or in order) each turn.")]
        public ComboData[] possibleCombos;
    }
}