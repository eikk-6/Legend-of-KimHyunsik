using System;
using System.IO;
using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class SaveLoadSystem : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private string saveFileName = "save_data.json";

        private string savePath;
        private bool suppressAutoSave;
        private bool seenStageEvent;
        private bool seenUpgradeEvent;
        private int lastKnownStage = -1;

        public string SavePath => savePath;
        public string LastLoadedQuitUtc { get; private set; } = string.Empty;

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            LoadAndApply();
        }

        private void OnEnable()
        {
            if (gameStateManager != null)
            {
                gameStateManager.StageChanged += OnStageChanged;
                lastKnownStage = gameStateManager.Stage;
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.UpgradesChanged += OnUpgradesChanged;
            }
        }

        private void OnDisable()
        {
            if (gameStateManager != null)
            {
                gameStateManager.StageChanged -= OnStageChanged;
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.UpgradesChanged -= OnUpgradesChanged;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        public void SaveNow()
        {
            if (string.IsNullOrWhiteSpace(savePath))
            {
                savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            }

            var data = new SaveData
            {
                stage = gameStateManager != null ? Mathf.Max(1, gameStateManager.Stage) : 1,
                gold = gameStateManager != null ? Mathf.Max(0, gameStateManager.Gold) : 0,
                attackLevel = upgradeSystem != null ? Mathf.Max(0, upgradeSystem.AttackLevel) : 0,
                attackSpeedLevel = upgradeSystem != null ? Mathf.Max(0, upgradeSystem.AttackSpeedLevel) : 0,
                critChanceLevel = upgradeSystem != null ? Mathf.Max(0, upgradeSystem.CritChanceLevel) : 0,
                lastQuitUtc = DateTime.UtcNow.ToString("O")
            };

            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(savePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveLoadSystem] Save failed: {exception.Message}");
            }
        }

        [ContextMenu("Debug Reset Progress")]
        public void ResetProgressForDebug()
        {
            if (string.IsNullOrWhiteSpace(savePath))
            {
                savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            }

            suppressAutoSave = true;

            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveLoadSystem] Failed to delete save file: {exception.Message}");
            }

            if (gameStateManager != null)
            {
                gameStateManager.OverrideStartingProgress(1, 0);
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.SetUpgradeLevels(0, 0, 0);
            }

            LastLoadedQuitUtc = string.Empty;
            seenStageEvent = false;
            seenUpgradeEvent = false;
            lastKnownStage = gameStateManager != null ? gameStateManager.Stage : -1;

            suppressAutoSave = false;
            SaveNow();

            Debug.Log("[SaveLoadSystem] Progress reset for debug.");
        }

        private void LoadAndApply()
        {
            var data = TryLoadFromDisk();
            if (data == null)
            {
                return;
            }

            suppressAutoSave = true;

            if (gameStateManager != null)
            {
                gameStateManager.OverrideStartingProgress(data.stage, data.gold);
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.SetUpgradeLevels(data.attackLevel, data.attackSpeedLevel, data.critChanceLevel);
            }

            LastLoadedQuitUtc = data.lastQuitUtc ?? string.Empty;
            suppressAutoSave = false;
        }

        private SaveData TryLoadFromDisk()
        {
            if (!File.Exists(savePath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(savePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    return null;
                }

                data.stage = Mathf.Max(1, data.stage);
                data.gold = Mathf.Max(0, data.gold);
                data.attackLevel = Mathf.Max(0, data.attackLevel);
                data.attackSpeedLevel = Mathf.Max(0, data.attackSpeedLevel);
                data.critChanceLevel = Mathf.Max(0, data.critChanceLevel);
                data.lastQuitUtc = data.lastQuitUtc ?? string.Empty;

                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveLoadSystem] Load failed. Starting defaults. Reason: {exception.Message}");
                return null;
            }
        }

        private void OnStageChanged(int stage)
        {
            if (!seenStageEvent)
            {
                seenStageEvent = true;
                lastKnownStage = stage;
                return;
            }

            if (stage == lastKnownStage)
            {
                return;
            }

            lastKnownStage = stage;
            AutoSaveIfAllowed();
        }

        private void OnUpgradesChanged()
        {
            if (!seenUpgradeEvent)
            {
                seenUpgradeEvent = true;
                return;
            }

            AutoSaveIfAllowed();
        }

        private void AutoSaveIfAllowed()
        {
            if (suppressAutoSave)
            {
                return;
            }

            SaveNow();
        }
    }
}
