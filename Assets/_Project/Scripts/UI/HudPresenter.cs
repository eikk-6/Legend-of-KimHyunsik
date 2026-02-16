using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Project.Core;

namespace Project.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text distanceText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Slider distanceSlider;

        private void OnEnable()
        {
            if (gameStateManager == null)
            {
                return;
            }

            gameStateManager.StageChanged += OnStageChanged;
            gameStateManager.DistanceChanged += OnDistanceChanged;
            gameStateManager.GoldChanged += OnGoldChanged;
            gameStateManager.StateChanged += OnStateChanged;

            OnStageChanged(gameStateManager.Stage);
            OnDistanceChanged(gameStateManager.Distance, 0f);
            OnGoldChanged(gameStateManager.Gold);
            OnStateChanged(gameStateManager.CurrentState, gameStateManager.CurrentState);
        }

        private void OnDisable()
        {
            if (gameStateManager == null)
            {
                return;
            }

            gameStateManager.StageChanged -= OnStageChanged;
            gameStateManager.DistanceChanged -= OnDistanceChanged;
            gameStateManager.GoldChanged -= OnGoldChanged;
            gameStateManager.StateChanged -= OnStateChanged;
        }

        private void OnStageChanged(int stage)
        {
            if (stageText != null)
            {
                stageText.text = $"Stage {stage}";
            }
        }

        private void OnDistanceChanged(float distance, float normalized)
        {
            if (distanceText != null)
            {
                distanceText.text = $"{distance:0.0} m";
            }

            if (distanceSlider != null)
            {
                distanceSlider.value = normalized;
            }
        }

        private void OnGoldChanged(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold {gold}";
            }
        }

        private void OnStateChanged(GameState _, GameState current)
        {
            if (stateText != null)
            {
                stateText.text = current.ToString().ToUpperInvariant();
            }
        }
    }
}
