using System;
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
        [SerializeField, Range(0f, 1f)] private float critChance = 0f;
        [SerializeField] private float critMultiplier = 2f;

        [Header("SFX")]
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip critClip;
        [SerializeField, Range(0.8f, 1.2f)] private float minPitch = 0.95f;
        [SerializeField, Range(0.8f, 1.2f)] private float maxPitch = 1.05f;

        public int AttackDamage => attackDamage;
        public float AttackSpeed => attackSpeed;
        public float CritChance => critChance;
        public float CritMultiplier => critMultiplier;
        public event Action<EnemyHealth, int, bool> HitLanded;

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

            var damage = Mathf.Max(1, attackDamage);
            var isCritical = UnityEngine.Random.value <= Mathf.Clamp01(critChance);
            if (isCritical)
            {
                damage = Mathf.Max(damage, Mathf.RoundToInt(damage * Mathf.Max(1f, critMultiplier)));
            }

            if (target.TakeDamage(damage, isCritical))
            {
                HitLanded?.Invoke(target, damage, isCritical);
                PlayHitSfx(isCritical);
            }
            cooldown = 1f / Mathf.Max(0.01f, attackSpeed);
        }

        public void SetAttackStats(int damage, float speed)
        {
            SetCombatStats(damage, speed, critChance, critMultiplier);
        }

        public void SetCombatStats(int damage, float speed, float newCritChance, float newCritMultiplier)
        {
            attackDamage = Mathf.Max(1, damage);
            attackSpeed = Mathf.Max(0.01f, speed);
            critChance = Mathf.Clamp01(newCritChance);
            critMultiplier = Mathf.Max(1f, newCritMultiplier);
        }

        private void PlayHitSfx(bool isCritical)
        {
            if (sfxAudioSource == null)
            {
                return;
            }

            var clip = isCritical && critClip != null ? critClip : hitClip;
            if (clip == null)
            {
                return;
            }

            var safeMinPitch = Mathf.Min(minPitch, maxPitch);
            var safeMaxPitch = Mathf.Max(minPitch, maxPitch);
            sfxAudioSource.pitch = UnityEngine.Random.Range(safeMinPitch, safeMaxPitch);
            sfxAudioSource.PlayOneShot(clip);
        }
    }
}
