using System.Collections.Generic;
using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private StageConfig stageConfig;

        [Header("References")]
        [SerializeField] private EnemyHealth waveEnemyPrefab;
        [SerializeField] private EnemyHealth bossEnemyPrefab;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform duelPoint;

        [Header("Wave (Fallback)")]
        [SerializeField] private int waveEnemyCount = 3;
        [SerializeField] private int baseEnemyHp = 15;
        [SerializeField] private int enemyHpPerStage = 3;
        [SerializeField] private int baseEnemyGold = 5;
        [SerializeField] private int enemyGoldPerStage = 1;

        [Header("Boss (Fallback)")]
        [SerializeField] private int baseBossHp = 120;
        [SerializeField] private float bossHpGrowth = 1.25f;
        [SerializeField] private int baseBossGold = 50;
        [SerializeField] private float bossGoldGrowth = 1.2f;

        [Header("Presentation")]
        [SerializeField] private float duelApproachSpeed = 8f;
        [SerializeField] private float queueApproachSpeed = 7f;
        [SerializeField] private float queueFrontOffset = 1.2f;
        [SerializeField] private float queueSpacing = 1.4f;
        [SerializeField] private float queueIdleBobAmplitude = 0.12f;
        [SerializeField] private float queueIdleBobFrequency = 3.2f;

        private readonly List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
        private bool bossEncounterActive;
        private EnemyHealth currentDuelEnemy;

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

            if (activeEnemies.Count > 0)
            {
                UpdateEnemyPresentation();
            }

            // Safety net: if enemies are gone but death callbacks were missed for any reason,
            // force encounter resolution so stage flow cannot get stuck.
            ResolveEncounterIfCleared();
        }

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
            var enemyHp = GetWaveEnemyHp(stage);
            var enemyGold = GetWaveEnemyGold(stage);
            var count = GetWaveEnemyCount(waveIndex);

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

            var hp = GetBossHp(stage);
            var gold = GetBossGold(stage);

            SpawnEnemies(bossEnemyPrefab, 1, hp, gold);
            bossEncounterActive = true;
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
                enemy.SetTargetable(false);
                if (enemy.GetComponent<EnemyHitFeedback>() == null)
                {
                    enemy.gameObject.AddComponent<EnemyHitFeedback>();
                }
                activeEnemies.Add(enemy);
            }

            ActivateNextDuelEnemy();
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
                if (currentDuelEnemy == enemy)
                {
                    currentDuelEnemy = null;
                }
            }

            ActivateNextDuelEnemy();
            ResolveEncounterIfCleared();
        }

        private void ActivateNextDuelEnemy()
        {
            if (currentDuelEnemy != null && !currentDuelEnemy.IsDead)
            {
                currentDuelEnemy.SetTargetable(true);
                SetWaitingEnemiesUntargetable(currentDuelEnemy);
                return;
            }

            currentDuelEnemy = null;

            for (var i = 0; i < activeEnemies.Count; i++)
            {
                var enemy = activeEnemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                currentDuelEnemy = enemy;
                currentDuelEnemy.SetTargetable(true);
                SetWaitingEnemiesUntargetable(currentDuelEnemy);
                return;
            }
        }

        private void UpdateEnemyPresentation()
        {
            var duelPosition = GetDuelPosition();

            if (currentDuelEnemy != null && !currentDuelEnemy.IsDead)
            {
                MoveEnemy(currentDuelEnemy, duelPosition, duelApproachSpeed);
            }

            var queueDirection = Vector3.right;
            var queueIndex = 0;
            for (var i = 0; i < activeEnemies.Count; i++)
            {
                var enemy = activeEnemies[i];
                if (enemy == null || enemy.IsDead || enemy == currentDuelEnemy)
                {
                    continue;
                }

                var targetPosition = duelPosition + (queueDirection * (queueFrontOffset + (queueSpacing * queueIndex)));
                var bob = Mathf.Sin((Time.time * queueIdleBobFrequency) + (enemy.GetInstanceID() * 0.01f)) * queueIdleBobAmplitude;
                targetPosition.y += bob;

                MoveEnemy(enemy, targetPosition, queueApproachSpeed);
                queueIndex++;
            }
        }

        private Vector3 GetDuelPosition()
        {
            if (duelPoint != null)
            {
                return duelPoint.position;
            }

            if (spawnRoot != null)
            {
                return spawnRoot.position + new Vector3(-3f, 0f, 0f);
            }

            return transform.position + new Vector3(-3f, 0f, 0f);
        }

        private void MoveEnemy(EnemyHealth enemy, Vector3 targetPosition, float speed)
        {
            if (enemy == null)
            {
                return;
            }

            var safeSpeed = Mathf.Max(0.01f, speed);
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, targetPosition, safeSpeed * Time.deltaTime);
        }

        private void SetWaitingEnemiesUntargetable(EnemyHealth activeEnemy)
        {
            for (var i = 0; i < activeEnemies.Count; i++)
            {
                var enemy = activeEnemies[i];
                if (enemy == null || enemy.IsDead || enemy == activeEnemy)
                {
                    continue;
                }

                enemy.SetTargetable(false);
            }
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
            currentDuelEnemy = null;
        }

        private int GetWaveEnemyCount(int waveIndex)
        {
            if (stageConfig != null)
            {
                return stageConfig.GetWaveEnemyCount(waveIndex);
            }

            return Mathf.Max(1, waveEnemyCount + Mathf.FloorToInt((waveIndex - 1) * 0.25f));
        }

        private int GetWaveEnemyHp(int stage)
        {
            if (stageConfig != null)
            {
                return stageConfig.GetWaveEnemyHp(stage);
            }

            return baseEnemyHp + Mathf.Max(0, stage - 1) * enemyHpPerStage;
        }

        private int GetWaveEnemyGold(int stage)
        {
            if (stageConfig != null)
            {
                return stageConfig.GetWaveEnemyGold(stage);
            }

            return baseEnemyGold + Mathf.Max(0, stage - 1) * enemyGoldPerStage;
        }

        private int GetBossHp(int stage)
        {
            if (stageConfig != null)
            {
                return stageConfig.GetBossHp(stage);
            }

            return Mathf.RoundToInt(baseBossHp * Mathf.Pow(Mathf.Max(1f, bossHpGrowth), Mathf.Max(0, stage - 1)));
        }

        private int GetBossGold(int stage)
        {
            if (stageConfig != null)
            {
                return stageConfig.GetBossGold(stage);
            }

            return Mathf.RoundToInt(baseBossGold * Mathf.Pow(Mathf.Max(1f, bossGoldGrowth), Mathf.Max(0, stage - 1)));
        }
    }
}
