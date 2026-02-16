using System.Collections;
using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class EnemyHitFeedback : MonoBehaviour
    {
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Visual")]
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.78f, 0.78f, 1f);
        [SerializeField] private Color critFlashColor = new Color(1f, 0.55f, 0.35f, 1f);
        [SerializeField] private float flashDuration = 0.08f;
        [SerializeField] private float punchScale = 0.15f;
        [SerializeField] private float critPunchScale = 0.28f;

        private Coroutine feedbackRoutine;
        private Vector3 initialLocalScale;
        private Color initialColor = Color.white;

        private void Awake()
        {
            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            initialLocalScale = transform.localScale;
            if (spriteRenderer != null)
            {
                initialColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.Damaged += OnDamaged;
            }
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.Damaged -= OnDamaged;
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
                feedbackRoutine = null;
            }

            transform.localScale = initialLocalScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = initialColor;
            }
        }

        private void OnDamaged(EnemyHealth _, int __, bool isCritical)
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(PlayHitFeedback(isCritical));
        }

        private IEnumerator PlayHitFeedback(bool isCritical)
        {
            var safeDuration = Mathf.Max(0.01f, flashDuration);
            var elapsed = 0f;
            var flashColor = isCritical ? critFlashColor : hitFlashColor;
            var punchAmount = isCritical ? critPunchScale : punchScale;

            while (elapsed < safeDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / safeDuration);
                var pulse = Mathf.Sin(t * Mathf.PI);

                transform.localScale = initialLocalScale * (1f + (punchAmount * pulse));
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(flashColor, initialColor, t);
                }

                yield return null;
            }

            transform.localScale = initialLocalScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = initialColor;
            }

            feedbackRoutine = null;
        }
    }
}
