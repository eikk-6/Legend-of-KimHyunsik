using Project.Core;
using Project.Data;
using Project.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public sealed class UpgradePanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private UpgradeSystem upgradeSystem;

        [Header("Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button attackSpeedButton;
        [SerializeField] private Button critChanceButton;

        [Header("Labels")]
        [SerializeField] private TMP_Text attackText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text critChanceText;

        [Header("SFX")]
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioClip purchaseClip;

        private void OnEnable()
        {
            if (attackButton != null)
            {
                attackButton.onClick.AddListener(OnClickAttack);
            }

            if (attackSpeedButton != null)
            {
                attackSpeedButton.onClick.AddListener(OnClickAttackSpeed);
            }

            if (critChanceButton != null)
            {
                critChanceButton.onClick.AddListener(OnClickCritChance);
            }

            if (gameStateManager != null)
            {
                gameStateManager.GoldChanged += OnGoldChanged;
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.UpgradesChanged += RefreshAll;
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            if (attackButton != null)
            {
                attackButton.onClick.RemoveListener(OnClickAttack);
            }

            if (attackSpeedButton != null)
            {
                attackSpeedButton.onClick.RemoveListener(OnClickAttackSpeed);
            }

            if (critChanceButton != null)
            {
                critChanceButton.onClick.RemoveListener(OnClickCritChance);
            }

            if (gameStateManager != null)
            {
                gameStateManager.GoldChanged -= OnGoldChanged;
            }

            if (upgradeSystem != null)
            {
                upgradeSystem.UpgradesChanged -= RefreshAll;
            }
        }

        private void OnGoldChanged(int _)
        {
            RefreshButtons();
        }

        private void OnClickAttack()
        {
            TryPurchase(UpgradeType.Attack);
        }

        private void OnClickAttackSpeed()
        {
            TryPurchase(UpgradeType.AttackSpeed);
        }

        private void OnClickCritChance()
        {
            TryPurchase(UpgradeType.CritChance);
        }

        private void TryPurchase(UpgradeType type)
        {
            if (upgradeSystem == null)
            {
                return;
            }

            if (upgradeSystem.TryPurchase(type))
            {
                PlayPurchaseSfx();
                RefreshAll();
                return;
            }

            RefreshButtons();
        }

        private void RefreshAll()
        {
            RefreshSlot(UpgradeType.Attack, attackButton, attackText, "ATK");
            RefreshSlot(UpgradeType.AttackSpeed, attackSpeedButton, attackSpeedText, "ASPD");
            RefreshSlot(UpgradeType.CritChance, critChanceButton, critChanceText, "CRIT");
        }

        private void RefreshButtons()
        {
            if (upgradeSystem == null)
            {
                if (attackButton != null)
                {
                    attackButton.interactable = false;
                }

                if (attackSpeedButton != null)
                {
                    attackSpeedButton.interactable = false;
                }

                if (critChanceButton != null)
                {
                    critChanceButton.interactable = false;
                }

                return;
            }

            if (attackButton != null)
            {
                attackButton.interactable = upgradeSystem.CanPurchase(UpgradeType.Attack);
            }

            if (attackSpeedButton != null)
            {
                attackSpeedButton.interactable = upgradeSystem.CanPurchase(UpgradeType.AttackSpeed);
            }

            if (critChanceButton != null)
            {
                critChanceButton.interactable = upgradeSystem.CanPurchase(UpgradeType.CritChance);
            }
        }

        private void RefreshSlot(UpgradeType type, Button button, TMP_Text text, string title)
        {
            if (upgradeSystem == null)
            {
                if (button != null)
                {
                    button.interactable = false;
                }

                if (text != null)
                {
                    text.text = title;
                }

                return;
            }

            var level = upgradeSystem.GetLevel(type);
            var cost = upgradeSystem.GetCost(type);
            var canPurchase = upgradeSystem.CanPurchase(type);

            if (button != null)
            {
                button.interactable = canPurchase;
            }

            if (text != null)
            {
                text.text = $"{title} Lv.{level}  Cost {FormatCompact(cost)}";
            }
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

        private void PlayPurchaseSfx()
        {
            if (sfxAudioSource == null || purchaseClip == null)
            {
                return;
            }

            sfxAudioSource.PlayOneShot(purchaseClip);
        }
    }
}
