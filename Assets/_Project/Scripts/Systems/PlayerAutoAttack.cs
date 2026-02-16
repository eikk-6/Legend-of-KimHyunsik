using Project.Core;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class PlayerAutoAttack : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private Transform attackOrigin;

        [Header("Stats")]
        [SerializeField] private int attackDamage = 5;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float attackRange = 12f;

        private float cooldown;

        private void Update()
        {
            if (gameStateManager == null)
            {
                return;
            }

            if (gameStateManager.CurrentState != GameState.Combat && gameStateManager.CurrentState != GameState.Boss)
            {
                return;
            }

            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }

            var origin = attackOrigin != null ? attackOrigin.position : transform.position;
            var target = EnemyRegistry.FindClosest(origin, Mathf.Max(0.1f, attackRange));
            if (target == null)
            {
                return;
            }

            target.TakeDamage(Mathf.Max(1, attackDamage));
            cooldown = 1f / Mathf.Max(0.01f, attackSpeed);
        }

        public void SetAttackStats(int damage, float speed)
        {
            attackDamage = Mathf.Max(1, damage);
            attackSpeed = Mathf.Max(0.01f, speed);
        }
    }
}
