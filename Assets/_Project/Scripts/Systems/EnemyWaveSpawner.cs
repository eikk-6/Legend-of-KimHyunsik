using System.Collections.Generic;
using Project.Core;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;

        [Header("References")]
        [SerializeField] private EnemyHealth waveEnemyPrefab;
        [SerializeField] private EnemyHealth bossEnemyPrefab;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private Transform[] spawnPoints;

        [Header("Wave")]
        [SerializeField] private int waveEnemyCount = 3;
        [SerializeField] private int baseEnemyHp = 15;
        [SerializeField] private int enemyHpPerStage = 3;
        [SerializeField] private int baseEnemyGold = 5;
        [SerializeField] private int enemyGoldPerStage = 1;

        [Header("Boss")]
        [SerializeField] private int baseBossHp = 120;
        [SerializeField] private float bossHpGrowth = 1.25f;
        [SerializeField] private int baseBossGold = 50;
        [SerializeField] private float bossGoldGrowth = 1.2f;

        private readonly List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
        private bool bossEncounterActive;

        private void OnEnable()
        {
            if (gameStateManager == null)
            {
                return;
            }

            gameStateManager.WaveEncounterTriggered += OnWaveEncounterTriggered;
            gameStateManager.BossEncounterTriggered += OnBossEncounterTriggered;
            gameStateManager.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (gameStateManager != null)
            {
                gameStateManager.WaveEncounterTriggered -= OnWaveEncounterTriggered;
                gameStateManager.BossEncounterTriggered -= OnBossEncounterTriggered;
                gameStateManager.StateChanged -= OnStateChanged;
            }

            ClearActiveEnemies();
        }

        private void OnWaveEncounterTriggered(int waveIndex, float _)
        {
            if (waveEnemyPrefab == null || gameStateManager == null)
            {
                gameStateManager?.ResolveCombatVictory();
                return;
            }

            bossEncounterActive = false;

            var stage = gameStateManager.Stage;
            var enemyHp = baseEnemyHp + Mathf.Max(0, stage - 1) * enemyHpPerStage;
            var enemyGold = baseEnemyGold + Mathf.Max(0, stage - 1) * enemyGoldPerStage;
            var count = Mathf.Max(1, waveEnemyCount + Mathf.FloorToInt((waveIndex - 1) * 0.25f));

            SpawnEnemies(waveEnemyPrefab, count, enemyHp, enemyGold);
            ResolveEncounterIfCleared();
        }

        private void OnBossEncounterTriggered(int stage)
        {
            if (bossEnemyPrefab == null || gameStateManager == null)
            {
                gameStateManager?.ResolveBossVictory();
                return;
            }

            bossEncounterActive = true;

            var hp = Mathf.RoundToInt(baseBossHp * Mathf.Pow(Mathf.Max(1f, bossHpGrowth), Mathf.Max(0, stage - 1)));
            var gold = Mathf.RoundToInt(baseBossGold * Mathf.Pow(Mathf.Max(1f, bossGoldGrowth), Mathf.Max(0, stage - 1)));

            SpawnEnemies(bossEnemyPrefab, 1, hp, gold);
            ResolveEncounterIfCleared();
        }

        private void OnStateChanged(GameState previous, GameState current)
        {
            if (current != GameState.Run || previous == GameState.Run)
            {
                return;
            }

            ClearActiveEnemies();
        }

        private void SpawnEnemies(EnemyHealth prefab, int count, int hp, int gold)
        {
            ClearActiveEnemies();

            for (var i = 0; i < count; i++)
            {
                var spawnPosition = GetSpawnPosition(i);
                var parent = spawnRoot != null ? spawnRoot : null;
                var enemy = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
                if (enemy == null)
                {
                    continue;
                }

                enemy.Configure(hp, gold);
                enemy.Died += OnEnemyDied;
                activeEnemies.Add(enemy);
            }
        }

        private Vector3 GetSpawnPosition(int index)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var point = spawnPoints[index % spawnPoints.Length];
                if (point != null)
                {
                    return point.position;
                }
            }

            var origin = spawnRoot != null ? spawnRoot.position : transform.position;
            return origin + new Vector3(8f + (index * 1.2f), 0f, 0f);
        }

        private void OnEnemyDied(EnemyHealth enemy)
        {
            if (enemy != null)
            {
                enemy.Died -= OnEnemyDied;
                gameStateManager?.AddGold(enemy.GoldReward);
                activeEnemies.Remove(enemy);
            }

            ResolveEncounterIfCleared();
        }

        private void ResolveEncounterIfCleared()
        {
            if (HasAliveEnemy())
            {
                return;
            }

            if (gameStateManager == null)
            {
                return;
            }

            if (bossEncounterActive && gameStateManager.CurrentState == GameState.Boss)
            {
                bossEncounterActive = false;
                gameStateManager.ResolveBossVictory();
                return;
            }

            if (gameStateManager.CurrentState == GameState.Combat)
            {
                gameStateManager.ResolveCombatVictory();
            }
        }

        private bool HasAliveEnemy()
        {
            for (var i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = activeEnemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                return true;
            }

            return false;
        }

        private void ClearActiveEnemies()
        {
            for (var i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = activeEnemies[i];
                if (enemy == null)
                {
                    continue;
                }

                enemy.Died -= OnEnemyDied;
                Destroy(enemy.gameObject);
            }

            activeEnemies.Clear();
            bossEncounterActive = false;
        }
    }
}
