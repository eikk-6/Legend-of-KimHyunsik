using TMPro;
using UnityEngine;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public sealed class FloatingDamageText : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float lifetime = 0.6f;
        [SerializeField] private float upwardSpeed = 1.4f;
        [SerializeField] private float driftSpeed = 0.35f;

        private Camera targetCamera;
        private Vector3 velocity;
        private float elapsed;
        private Color startColor = Color.white;

        public void Initialize(string content, Color color, Vector3 startVelocity, Camera worldCamera)
        {
            if (text == null)
            {
                text = GetComponent<TMP_Text>();
            }

            if (text != null)
            {
                text.text = content;
                text.color = color;
            }

            startColor = color;
            velocity = new Vector3(startVelocity.x * driftSpeed, upwardSpeed + startVelocity.y, 0f);
            targetCamera = worldCamera != null ? worldCamera : Camera.main;
            elapsed = 0f;
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            elapsed += dt;

            transform.position += velocity * dt;
            velocity *= 0.98f;

            if (targetCamera != null)
            {
                transform.forward = targetCamera.transform.forward;
            }

            if (text != null)
            {
                var t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, lifetime));
                var c = startColor;
                c.a = 1f - t;
                text.color = c;
            }

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
