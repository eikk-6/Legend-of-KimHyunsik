using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Project/Config/Upgrade Config")]
    public sealed class UpgradeConfig : ScriptableObject
    {
        [Header("Economy")]
        [SerializeField] private float costGrowth = 1.15f;
        [SerializeField] private int attackBaseCost = 20;
        [SerializeField] private int attackSpeedBaseCost = 20;
        [SerializeField] private int critChanceBaseCost = 25;

        [Header("Base Stats")]
        [SerializeField] private int baseAttack = 5;
        [SerializeField] private float baseAttackSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float baseCritChance;
        [SerializeField] private float critMultiplier = 2f;

        [Header("Per Level Gain")]
        [SerializeField] private int attackPerLevel = 1;
        [SerializeField] private float attackSpeedPerLevel = 0.1f;
        [SerializeField, Range(0f, 1f)] private float critChancePerLevel = 0.01f;
        [SerializeField, Range(0f, 1f)] private float critChanceCap = 0.75f;

        public float CostGrowth => Mathf.Max(1f, costGrowth);
        public float CritMultiplier => Mathf.Max(1f, critMultiplier);

        public int GetBaseCost(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Attack:
                    return Mathf.Max(1, attackBaseCost);
                case UpgradeType.AttackSpeed:
                    return Mathf.Max(1, attackSpeedBaseCost);
                case UpgradeType.CritChance:
                    return Mathf.Max(1, critChanceBaseCost);
                default:
                    return Mathf.Max(1, attackBaseCost);
            }
        }

        public int CalculateCost(UpgradeType type, int level)
        {
            var safeLevel = Mathf.Max(0, level);
            return Mathf.Max(1, Mathf.CeilToInt(GetBaseCost(type) * Mathf.Pow(CostGrowth, safeLevel)));
        }

        public int CalculateAttack(int level)
        {
            return Mathf.Max(1, baseAttack + Mathf.Max(0, level) * attackPerLevel);
        }

        public float CalculateAttackSpeed(int level)
        {
            return Mathf.Max(0.01f, baseAttackSpeed + Mathf.Max(0, level) * attackSpeedPerLevel);
        }

        public float CalculateCritChance(int level)
        {
            var chance = baseCritChance + Mathf.Max(0, level) * critChancePerLevel;
            chance = Mathf.Clamp01(chance);
            return Mathf.Min(chance, Mathf.Clamp01(critChanceCap));
        }
    }
}
