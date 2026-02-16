using System;
using UnityEngine;

namespace Project.Core
{
    public enum GameState
    {
        Run = 0,
        Combat = 1,
        Boss = 2,
        StageClear = 3
    }

    public sealed class GameStateManager : MonoBehaviour
    {
        [Header("Run")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float stageLength = 100f;
        [SerializeField] private float spawnInterval = 10f;

        [Header("Progression")]
        [SerializeField] private int startingStage = 1;
        [SerializeField] private int startingGold = 0;

        public event Action<GameState, GameState> StateChanged;
        public event Action<int> StageChanged;
        public event Action<int> GoldChanged;
        public event Action<float, float> DistanceChanged;
        public event Action<int, float> WaveEncounterTriggered;
        public event Action<int> BossEncounterTriggered;

        public GameState CurrentState { get; private set; } = GameState.Run;
        public int Stage { get; private set; }
        public int Gold { get; private set; }
        public float Distance { get; private set; }

        private float _nextWaveDistance;
        private bool _bossTriggered;

        private void Start()
        {
            Stage = startingStage;
            Gold = Mathf.Max(0, startingGold);

            StageChanged?.Invoke(Stage);
            GoldChanged?.Invoke(Gold);

            ResetStageProgress();
            ChangeState(GameState.Run);
        }

        private void Update()
        {
            if (CurrentState != GameState.Run)
            {
                return;
            }

            AdvanceRun();
        }

        public void ResolveCombatVictory()
        {
            if (CurrentState != GameState.Combat)
            {
                return;
            }

            ChangeState(GameState.Run);
        }

        public void ResolveBossVictory()
        {
            if (CurrentState != GameState.Boss)
            {
                return;
            }

            ChangeState(GameState.StageClear);
            CompleteStage();
        }

        public void EnterCombat()
        {
            ChangeState(GameState.Combat);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Gold += amount;
            GoldChanged?.Invoke(Gold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            GoldChanged?.Invoke(Gold);
            return true;
        }

        private void AdvanceRun()
        {
            var safeStageLength = Mathf.Max(0.01f, stageLength);
            var safeSpawnInterval = Mathf.Max(0.1f, spawnInterval);

            Distance += moveSpeed * Time.deltaTime;
            var normalizedDistance = Mathf.Clamp01(Distance / safeStageLength);
            DistanceChanged?.Invoke(Distance, normalizedDistance);

            if (!_bossTriggered && Distance >= safeStageLength)
            {
                _bossTriggered = true;
                ChangeState(GameState.Boss);
                BossEncounterTriggered?.Invoke(Stage);
                return;
            }

            if (Distance < _nextWaveDistance || _nextWaveDistance >= safeStageLength)
            {
                return;
            }

            var encounterDistance = _nextWaveDistance;
            var waveIndex = Mathf.Max(1, Mathf.FloorToInt(encounterDistance / safeSpawnInterval));
            _nextWaveDistance += safeSpawnInterval;
            ChangeState(GameState.Combat);
            WaveEncounterTriggered?.Invoke(waveIndex, encounterDistance);
        }

        private void CompleteStage()
        {
            Stage += 1;
            StageChanged?.Invoke(Stage);

            ResetStageProgress();
            ChangeState(GameState.Run);
        }

        private void ResetStageProgress()
        {
            Distance = 0f;
            _nextWaveDistance = Mathf.Max(0.1f, spawnInterval);
            _bossTriggered = false;
            DistanceChanged?.Invoke(Distance, 0f);
        }

        private void ChangeState(GameState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            var prevState = CurrentState;
            CurrentState = nextState;
            StateChanged?.Invoke(prevState, CurrentState);
        }
    }
}
