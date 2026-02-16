using System;
using System.Collections;
using System.Globalization;
using Project.Core;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class OfflineRewardSystem : MonoBehaviour
    {
        [SerializeField] private SaveLoadSystem saveLoadSystem;
        [SerializeField] private GameStateManager gameStateManager;

        [Header("Reward")]
        [SerializeField] private int goldPerMinute = 10;
        [SerializeField] private float maxOfflineHours = 8f;
        [SerializeField] private bool grantOnStart = true;

        public event Action<int, TimeSpan> RewardGranted;

        public int LastGrantedGold { get; private set; }
        public TimeSpan LastElapsed { get; private set; } = TimeSpan.Zero;

        private bool hasProcessed;

        private IEnumerator Start()
        {
            // Wait one frame so GameStateManager/Upgrade load initialization is complete.
            yield return null;

            if (grantOnStart)
            {
                TryGrantReward();
            }
        }

        public bool TryGrantReward()
        {
            if (hasProcessed)
            {
                return false;
            }

            hasProcessed = true;
            LastGrantedGold = 0;
            LastElapsed = TimeSpan.Zero;

            if (saveLoadSystem == null || gameStateManager == null)
            {
                return false;
            }

            if (!TryParseUtc(saveLoadSystem.LastLoadedQuitUtc, out var lastQuitUtc))
            {
                return false;
            }

            var nowUtc = DateTime.UtcNow;
            if (nowUtc <= lastQuitUtc)
            {
                return false;
            }

            var elapsed = nowUtc - lastQuitUtc;
            var cap = TimeSpan.FromHours(Mathf.Max(0f, maxOfflineHours));
            if (elapsed > cap)
            {
                elapsed = cap;
            }

            var safePerMinute = Mathf.Max(0, goldPerMinute);
            var reward = Mathf.FloorToInt((float)elapsed.TotalMinutes * safePerMinute);
            if (reward <= 0)
            {
                return false;
            }

            gameStateManager.AddGold(reward);
            LastGrantedGold = reward;
            LastElapsed = elapsed;

            RewardGranted?.Invoke(reward, elapsed);
            saveLoadSystem.SaveNow();
            return true;
        }

        private static bool TryParseUtc(string utcText, out DateTime utc)
        {
            utc = default;
            if (string.IsNullOrWhiteSpace(utcText))
            {
                return false;
            }

            return DateTime.TryParse(
                utcText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out utc);
        }
    }
}
