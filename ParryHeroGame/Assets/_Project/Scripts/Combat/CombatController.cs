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

        [Header("Runtime State (read-only)")]
        [SerializeField] private int heroHealth;
        [SerializeField] private int enemyHealth;

        private AttackResolver resolver;

        private void Awake()
        {
            resolver = gameObject.AddComponent<AttackResolver>();
        }

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

            if (heroHealth <= 0) Debug.Log("=== HERO DIED ===");
            else Debug.Log("=== ENEMY DIED ===");
        }

        private IEnumerator RunTurn(TurnOwner owner)
        {
            ComboData combo = PickCombo(owner);
            if (combo == null || combo.attacks.Length == 0) yield break;

            Debug.Log($"--- {owner} turn start ({combo.attacks.Length} hits) ---");

            foreach (AttackData attack in combo.attacks)
            {
                // Enemy turn -> player listens on Block button (to Parry/Block)
                // Hero turn  -> player listens on Attack button (to land the hit)
                bool listenForAttackButton = owner == TurnOwner.Hero;

                HitResult result = HitResult.Miss;
                bool done = false;

                IEnumerator routine = attack.signalType == SignalType.Simple
                    ? resolver.ResolveSimple(attack, listenForAttackButton, r => { result = r; done = true; })
                    : resolver.ResolveCharged(attack, listenForAttackButton, r => { result = r; done = true; });

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
                // Enemy attacking: Miss = player failed to Parry/Block -> hero takes damage
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
                // Hero attacking: Perfect/Good = hit lands -> enemy takes damage. Miss = no damage.
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
            // TODO: Hero combos will come from a HeroData asset later.
            // For now, only Enemy combos are wired up via enemyData.
            if (owner != TurnOwner.Enemy) return debugHeroCombo;

            if (enemyData.possibleCombos == null || enemyData.possibleCombos.Length == 0) return null;
            int index = Random.Range(0, enemyData.possibleCombos.Length);
            return enemyData.possibleCombos[index];
        }

        [Header("TEMP - Hero combo for testing (until HeroData exists)")]
        public ComboData debugHeroCombo;
    }
}