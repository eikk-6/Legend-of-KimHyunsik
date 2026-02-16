using Project.Systems;
using TMPro;
using UnityEngine;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public sealed class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerAutoAttack playerAutoAttack;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private TMP_FontAsset fontAsset;

        [Header("Text")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
        [SerializeField] private float fontSize = 4f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = new Color(1f, 0.75f, 0.3f, 1f);
        [SerializeField] private float criticalScale = 1.25f;

        [Header("Scatter")]
        [SerializeField] private Vector2 randomX = new Vector2(-0.2f, 0.2f);
        [SerializeField] private Vector2 randomY = new Vector2(0.1f, 0.35f);

        private Transform container;

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (fontAsset == null)
            {
                fontAsset = TMP_Settings.defaultFontAsset;
            }
        }

        private void OnEnable()
        {
            if (playerAutoAttack != null)
            {
                playerAutoAttack.HitLanded += OnHitLanded;
            }
        }

        private void OnDisable()
        {
            if (playerAutoAttack != null)
            {
                playerAutoAttack.HitLanded -= OnHitLanded;
            }
        }

        private void OnHitLanded(EnemyHealth target, int damage, bool isCritical)
        {
            if (target == null)
            {
                return;
            }

            if (container == null)
            {
                var go = new GameObject("DamageTextContainer");
                container = go.transform;
            }

            var damageTextObject = new GameObject("DamageText");
            damageTextObject.transform.SetParent(container, false);
            damageTextObject.transform.position = target.transform.position + worldOffset;

            var tmp = damageTextObject.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            tmp.fontSize = isCritical ? fontSize * criticalScale : fontSize;
            tmp.font = fontAsset != null ? fontAsset : TMP_Settings.defaultFontAsset;

            var textRenderer = damageTextObject.GetComponent<Renderer>();
            var targetRenderer = target.GetComponentInChildren<Renderer>();
            if (textRenderer != null && targetRenderer != null)
            {
                textRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                textRenderer.sortingOrder = targetRenderer.sortingOrder + 5;
            }

            var behavior = damageTextObject.AddComponent<FloatingDamageText>();
            var color = isCritical ? criticalColor : normalColor;
            var velocity = new Vector3(
                Random.Range(randomX.x, randomX.y),
                Random.Range(randomY.x, randomY.y),
                0f);

            behavior.Initialize(damage.ToString(), color, velocity, worldCamera);
        }
    }
}
