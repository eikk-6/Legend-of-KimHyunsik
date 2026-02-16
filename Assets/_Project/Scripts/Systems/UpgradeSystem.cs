using System;
using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class UpgradeSystem : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private PlayerAutoAttack playerAutoAttack;
        [SerializeField] private UpgradeConfig upgradeConfig;

        [Header("Economy (Fallback)")]
        [SerializeField] private float costGrowth = 1.15f;
        [SerializeField] private int attackBaseCost = 20;
        [SerializeField] private int attackSpeedBaseCost = 20;
        [SerializeField] private int critChanceBaseCost = 25;

        [Header("Base Stats (Fallback)")]
        [SerializeField] private int baseAttack = 5;
        [SerializeField] private float baseAttackSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float baseCritChance = 0f;
        [SerializeField] private float critMultiplier = 2f;

        [Header("Per Level Gain (Fallback)")]
        [SerializeField] private int attackPerLevel = 1;
        [SerializeField] private float attackSpeedPerLevel = 0.1f;
        [SerializeField, Range(0f, 1f)] private float critChancePerLevel = 0.01f;
        [SerializeField, Range(0f, 1f)] private float critChanceCap = 0.75f;

        public event Action UpgradesChanged;

        public int AttackLevel { get; private set; }
        public int AttackSpeedLevel { get; private set; }
        public int CritChanceLevel { get; private set; }

        public int CurrentAttack => CalculateAttack(AttackLevel);
        public float CurrentAttackSpeed => CalculateAttackSpeed(AttackSpeedLevel);
        public float CurrentCritChance => CalculateCritChance(CritChanceLevel);
        public float CurrentCritMultiplier => GetCritMultiplier();

        private void Start()
        {
            ApplyStatsToPlayer();
            UpgradesChanged?.Invoke();
        }

        public int GetLevel(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Attack:
                    return AttackLevel;
                case UpgradeType.AttackSpeed:
                    return AttackSpeedLevel;
                case UpgradeType.CritChance:
                    return CritChanceLevel;
                default:
                    return 0;
            }
        }

        public int GetCost(UpgradeType type)
        {
            var level = Mathf.Max(0, GetLevel(type));
            if (upgradeConfig != null)
            {
                return upgradeConfig.CalculateCost(type, level);
            }

            var baseCost = Mathf.Max(1, GetBaseCost(type));
            var safeGrowth = Mathf.Max(1f, costGrowth);
            return Mathf.Max(1, Mathf.CeilToInt(baseCost * Mathf.Pow(safeGrowth, level)));
        }

        public bool CanPurchase(UpgradeType type)
        {
            if (gameStateManager == null)
            {
                return false;
            }

            return gameStateManager.Gold >= GetCost(type);
        }

        public bool TryPurchase(UpgradeType type)
        {
            if (gameStateManager == null)
            {
                return false;
            }

            var cost = GetCost(type);
            if (!gameStateManager.TrySpendGold(cost))
            {
                return false;
            }

            IncreaseLevel(type);
            ApplyStatsToPlayer();
            UpgradesChanged?.Invoke();
            return true;
        }

        public void SetUpgradeLevels(int attack, int attackSpeed, int critChance)
        {
            AttackLevel = Mathf.Max(0, attack);
            AttackSpeedLevel = Mathf.Max(0, attackSpeed);
            CritChanceLevel = Mathf.Max(0, critChance);

            ApplyStatsToPlayer();
            UpgradesChanged?.Invoke();
        }

        private int GetBaseCost(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Attack:
                    return attackBaseCost;
                case UpgradeType.AttackSpeed:
                    return attackSpeedBaseCost;
                case UpgradeType.CritChance:
                    return critChanceBaseCost;
                default:
                    return attackBaseCost;
            }
        }

        private void IncreaseLevel(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Attack:
                    AttackLevel += 1;
                    break;
                case UpgradeType.AttackSpeed:
                    AttackSpeedLevel += 1;
                    break;
                case UpgradeType.CritChance:
                    CritChanceLevel += 1;
                    break;
            }
        }

        private int CalculateAttack(int level)
        {
            if (upgradeConfig != null)
            {
                return upgradeConfig.CalculateAttack(level);
            }

            return Mathf.Max(1, baseAttack + Mathf.Max(0, level) * attackPerLevel);
        }

        private float CalculateAttackSpeed(int level)
        {
            if (upgradeConfig != null)
            {
                return upgradeConfig.CalculateAttackSpeed(level);
            }

            return Mathf.Max(0.01f, baseAttackSpeed + Mathf.Max(0, level) * attackSpeedPerLevel);
        }

        private float CalculateCritChance(int level)
        {
            if (upgradeConfig != null)
            {
                return upgradeConfig.CalculateCritChance(level);
            }

            var chance = baseCritChance + Mathf.Max(0, level) * critChancePerLevel;
            chance = Mathf.Clamp01(chance);
            return Mathf.Min(chance, Mathf.Clamp01(critChanceCap));
        }

        private float GetCritMultiplier()
        {
            if (upgradeConfig != null)
            {
                return upgradeConfig.CritMultiplier;
            }

            return Mathf.Max(1f, critMultiplier);
        }

        private void ApplyStatsToPlayer()
        {
            if (playerAutoAttack == null)
            {
                return;
            }

            playerAutoAttack.SetCombatStats(
                CurrentAttack,
                CurrentAttackSpeed,
                CurrentCritChance,
                CurrentCritMultiplier);
        }
    }
}
