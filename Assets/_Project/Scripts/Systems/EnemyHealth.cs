using System;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHp = 10;
        [SerializeField] private int goldReward = 1;
        [SerializeField] private bool destroyOnDeath = true;

        public event Action<EnemyHealth> Died;
        public event Action<EnemyHealth, int, bool> Damaged;

        public int MaxHp { get; private set; }
        public int CurrentHp { get; private set; }
        public int GoldReward => goldReward;
        public bool IsDead { get; private set; }
        public bool CanBeTargeted { get; private set; } = true;

        private void Awake()
        {
            ResetStats();
        }

        private void OnEnable()
        {
            EnemyRegistry.Register(this);
        }

        private void OnDisable()
        {
            EnemyRegistry.Unregister(this);
        }

        public void Configure(int hp, int reward)
        {
            MaxHp = Mathf.Max(1, hp);
            CurrentHp = MaxHp;
            goldReward = Mathf.Max(0, reward);
            IsDead = false;
            CanBeTargeted = true;
        }

        public void ResetStats()
        {
            MaxHp = Mathf.Max(1, maxHp);
            CurrentHp = MaxHp;
            IsDead = false;
            CanBeTargeted = true;
        }

        public void SetTargetable(bool targetable)
        {
            if (IsDead)
            {
                CanBeTargeted = false;
                return;
            }

            CanBeTargeted = targetable;
        }

        public bool TakeDamage(int damage, bool isCritical = false)
        {
            if (IsDead || damage <= 0)
            {
                return false;
            }

            CurrentHp = Mathf.Max(0, CurrentHp - damage);
            Damaged?.Invoke(this, damage, isCritical);
            if (CurrentHp > 0)
            {
                return true;
            }

            Die();
            return true;
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            Died?.Invoke(this);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
