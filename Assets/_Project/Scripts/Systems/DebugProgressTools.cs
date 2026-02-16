using UnityEngine;

namespace Project.Systems
{
    [DisallowMultipleComponent]
    public sealed class DebugProgressTools : MonoBehaviour
    {
        [SerializeField] private SaveLoadSystem saveLoadSystem;
        [SerializeField] private KeyCode resetProgressKey = KeyCode.F8;
        [SerializeField] private bool requireShift = true;

        private void Update()
        {
            if (saveLoadSystem == null)
            {
                return;
            }

            if (!Input.GetKeyDown(resetProgressKey))
            {
                return;
            }

            if (requireShift && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                return;
            }

            saveLoadSystem.ResetProgressForDebug();
        }
    }
}
