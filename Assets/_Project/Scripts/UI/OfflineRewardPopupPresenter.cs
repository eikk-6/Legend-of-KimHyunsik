using System;
using Project.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public sealed class OfflineRewardPopupPresenter : MonoBehaviour
    {
        [SerializeField] private OfflineRewardSystem offlineRewardSystem;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text elapsedText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            SetPopupVisible(false);
        }

        private void OnEnable()
        {
            if (offlineRewardSystem != null)
            {
                offlineRewardSystem.RewardGranted += OnRewardGranted;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnClickClose);
            }
        }

        private void OnDisable()
        {
            if (offlineRewardSystem != null)
            {
                offlineRewardSystem.RewardGranted -= OnRewardGranted;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnClickClose);
            }
        }

        private void OnRewardGranted(int gold, TimeSpan elapsed)
        {
            if (titleText != null)
            {
                titleText.text = "Offline Reward";
            }

            if (rewardText != null)
            {
                rewardText.text = $"+{FormatCompact(gold)} Gold";
            }

            if (elapsedText != null)
            {
                elapsedText.text = $"Elapsed: {FormatDuration(elapsed)}";
            }

            SetPopupVisible(true);
        }

        private void OnClickClose()
        {
            SetPopupVisible(false);
        }

        private void SetPopupVisible(bool visible)
        {
            if (popupRoot == null)
            {
                return;
            }

            // Avoid disabling this presenter object itself.
            if (popupRoot == gameObject)
            {
                return;
            }

            if (popupRoot.activeSelf != visible)
            {
                popupRoot.SetActive(visible);
            }
        }

        private static string FormatDuration(TimeSpan elapsed)
        {
            if (elapsed.TotalHours >= 1d)
            {
                return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
            }

            return $"{Mathf.Max(0, elapsed.Minutes)}m";
        }

        private static string FormatCompact(int value)
        {
            if (value >= 1_000_000_000)
            {
                return $"{value / 1_000_000_000f:0.0}B";
            }

            if (value >= 1_000_000)
            {
                return $"{value / 1_000_000f:0.0}M";
            }

            if (value >= 1_000)
            {
                return $"{value / 1_000f:0.0}K";
            }

            return value.ToString();
        }
    }
}
