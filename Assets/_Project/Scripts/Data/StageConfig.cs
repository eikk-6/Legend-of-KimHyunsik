using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(fileName = "StageConfig", menuName = "Project/Config/Stage Config")]
    public sealed class StageConfig : ScriptableObject
    {
        [Header("Run")]
        [SerializeField] private float stageLength = 100f;
        [SerializeField] private float spawnInterval = 10f;

        [Header("Wave")]
        [SerializeField] private int baseWaveEnemyCount = 3;
        [SerializeField] private float waveCountGrowthPerWave = 0.25f;
        [SerializeField] private int baseEnemyHp = 15;
        [SerializeField] private int enemyHpPerStage = 3;
        [SerializeField] private int baseEnemyGold = 5;
        [SerializeField] private int enemyGoldPerStage = 1;

        [Header("Boss")]
        [SerializeField] private int baseBossHp = 120;
        [SerializeField] private float bossHpGrowth = 1.25f;
        [SerializeField] private int baseBossGold = 50;
        [SerializeField] private float bossGoldGrowth = 1.2f;

        public float StageLength => Mathf.Max(0.01f, stageLength);
        public float SpawnInterval => Mathf.Max(0.1f, spawnInterval);

        public int GetWaveEnemyCount(int waveIndex)
        {
            var safeWaveIndex = Mathf.Max(1, waveIndex);
            var additional = Mathf.FloorToInt((safeWaveIndex - 1) * Mathf.Max(0f, waveCountGrowthPerWave));
            return Mathf.Max(1, baseWaveEnemyCount + additional);
        }

        public int GetWaveEnemyHp(int stage)
        {
            return Mathf.Max(1, baseEnemyHp + (Mathf.Max(1, stage) - 1) * enemyHpPerStage);
        }

        public int GetWaveEnemyGold(int stage)
        {
            return Mathf.Max(0, baseEnemyGold + (Mathf.Max(1, stage) - 1) * enemyGoldPerStage);
        }

        public int GetBossHp(int stage)
        {
            var safeStage = Mathf.Max(1, stage);
            var value = baseBossHp * Mathf.Pow(Mathf.Max(1f, bossHpGrowth), safeStage - 1);
            return Mathf.Max(1, Mathf.RoundToInt(value));
        }

        public int GetBossGold(int stage)
        {
            var safeStage = Mathf.Max(1, stage);
            var value = baseBossGold * Mathf.Pow(Mathf.Max(1f, bossGoldGrowth), safeStage - 1);
            return Mathf.Max(0, Mathf.RoundToInt(value));
        }
    }
}
