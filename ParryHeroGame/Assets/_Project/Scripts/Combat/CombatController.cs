using System.Collections;
using UnityEngine;
using CombatGame.Data;

namespace CombatGame.Combat
{
    public class CombatController : MonoBehaviour
    {
        [Header("Setup")]
        public EnemyData enemyData;
        public int heroMaxHealth = 10;

        [Header("Characters")]
        public CharacterAnimationController heroAnimation;
        public CharacterAnimationController enemyAnimation;

        [Header("Runtime State (read-only)")]
        [SerializeField] private int heroHealth;
        [SerializeField] private int enemyHealth;

        [Header("Hero Attack Feedback")]
        public MissTextUI missTextUI; 

        public AttackResolver resolver;

        private void Start()
        {
            heroHealth = heroMaxHealth;
            enemyHealth = enemyData.maxHealth;
            StartCoroutine(CombatLoop());
        }

        private IEnumerator CombatLoop()
        {
            Debug.Log("=== COMBAT START ===");

            while (heroHealth > 0 && enemyHealth > 0)
            {
                yield return StartCoroutine(RunTurn(TurnOwner.Enemy));
                if (heroHealth <= 0) break;

                yield return StartCoroutine(RunTurn(TurnOwner.Hero));
                if (enemyHealth <= 0) break;
            }

            if (heroHealth <= 0)
            {
                heroAnimation.PlayDeath();
                Debug.Log("=== HERO DIED ===");
            }
            else
            {
                enemyAnimation.PlayDeath();
                Debug.Log("=== ENEMY DIED ===");
            }
        }

        private IEnumerator RunTurn(TurnOwner owner)
        {
            ComboData combo = PickCombo(owner);
            if (combo == null || combo.attacks.Length == 0) yield break;

            Debug.Log($"--- {owner} turn start ({combo.attacks.Length} hits) ---");

            foreach (AttackData attack in combo.attacks)
            {
                HitResult result = HitResult.Miss;
                bool done = false;

                IEnumerator routine;
                if (owner == TurnOwner.Hero)
                {
                    routine = attack.signalType == SignalType.Simple
                        ? resolver.ResolveHeroAttackSimple(attack, heroAnimation, enemyAnimation, r => { result = r; done = true; })
                        : resolver.ResolveHeroAttackCombo(attack, heroAnimation, enemyAnimation, r => { result = r; done = true; });
                }
                else
                {
                    routine = attack.signalType == SignalType.Simple
                        ? resolver.ResolveSimple(attack, false, enemyAnimation, heroAnimation, r => { result = r; done = true; })
                        : resolver.ResolveCharged(attack, false, enemyAnimation, heroAnimation, r => { result = r; done = true; });
                }

                yield return StartCoroutine(routine);
                while (!done) yield return null;

                ApplyResult(owner, attack, result);

                if (heroHealth <= 0 || enemyHealth <= 0) yield break;
            }

            Debug.Log($"--- {owner} turn end, pause {combo.endOfComboPause}s ---");
            yield return new WaitForSeconds(combo.endOfComboPause);
        }

        private void ApplyResult(TurnOwner owner, AttackData attack, HitResult result)
        {
            if (owner == TurnOwner.Enemy)
            {
                if (result == HitResult.Miss)
                {
                    heroHealth -= attack.damageOnFail;
                    Debug.Log($"[Damage] Hero takes {attack.damageOnFail} -> {heroHealth} HP left");
                }
                else
                {
                    Debug.Log($"[Block] Hero blocked successfully ({result})");
                }
            }
            else
            {
                if (result != HitResult.Miss)
                {
                    int dmg = attack.signalType == SignalType.Charged ? attack.damageOnFail : 1;
                    enemyHealth -= dmg;
                    Debug.Log($"[Damage] Enemy takes {dmg} -> {enemyHealth} HP left");
                }
                else
                {
                    Debug.Log("[Miss] Hero's attack missed, no damage dealt");
                }
            }
        }

        private ComboData PickCombo(TurnOwner owner)
        {
            if (owner != TurnOwner.Enemy) return debugHeroCombo;

            if (enemyData.possibleCombos == null || enemyData.possibleCombos.Length == 0) return null;
            int index = Random.Range(0, enemyData.possibleCombos.Length);
            return enemyData.possibleCombos[index];
        }

        [Header("TEMP - Hero combo for testing (until HeroData exists)")]
        public ComboData debugHeroCombo;
    }
}