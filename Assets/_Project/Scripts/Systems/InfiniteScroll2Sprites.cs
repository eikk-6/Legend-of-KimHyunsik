using Project.Core;
using UnityEngine;

namespace Project.Systems
{
    /// <summary>
    /// Infinite horizontal scrolling with 2 identical sprites.
    /// Moves both left, then teleports the off-screen one to the right side.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InfiniteScroll2Sprites : MonoBehaviour
    {
        [Header("Sprite Renderers (same sprite)")]
        [SerializeField] private SpriteRenderer a;
        [SerializeField] private SpriteRenderer b;

        [Header("Scroll")]
        [SerializeField] private float speed = 2f;

        [Header("Optional State Gating")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private bool scrollOnlyInRunState = true;

        private float width;
        private Camera cam;

        private void Awake()
        {
            if (a == null || b == null)
            {
                Debug.LogError($"{name}: SpriteRenderer references are missing.");
                enabled = false;
                return;
            }

            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError($"{name}: Main Camera not found. Check MainCamera tag.");
                enabled = false;
                return;
            }

            width = a.bounds.size.x;
            if (width <= 0f)
            {
                Debug.LogError($"{name}: Sprite width is invalid.");
                enabled = false;
                return;
            }

            // Snap b exactly to the right side of a on start.
            b.transform.position = a.transform.position + Vector3.right * width;
        }

        private void Update()
        {
            if (scrollOnlyInRunState &&
                gameStateManager != null &&
                gameStateManager.CurrentState != GameState.Run)
            {
                return;
            }

            var dt = Time.deltaTime;
            var delta = Vector3.left * speed * dt;

            a.transform.position += delta;
            b.transform.position += delta;

            var camLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;

            var left = a.transform.position.x <= b.transform.position.x ? a : b;
            var right = left == a ? b : a;

            var leftRightEdge = left.transform.position.x + (width * 0.5f);
            if (leftRightEdge >= camLeft)
            {
                return;
            }

            var rightRightEdge = right.transform.position.x + (width * 0.5f);
            left.transform.position = new Vector3(
                rightRightEdge + (width * 0.5f),
                left.transform.position.y,
                left.transform.position.z);
        }
    }
}
