using UnityEngine;

namespace VerdantRemains.Core
{
    public sealed class DevToggles : MonoBehaviour
    {
        [Tooltip("Press this key to toggle all target GameObjects on/off.")]
        public KeyCode key = KeyCode.F2;

        [Tooltip("Any objects you want to show/hide (e.g., DebugOverlay, Heatmap).")]
        public GameObject[] toggleTargets;

        void Update()
        {
            if (Input.GetKeyDown(key) && toggleTargets != null)
            {
                foreach (var go in toggleTargets)
                {
                    if (!go) continue;
                    go.SetActive(!go.activeSelf);
                }
            }
        }
    }
}
