using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace VerdantRemains.Core
{
    /// <summary>
    /// F2 (or any binding you choose) toggles target objects on/off.
    /// New Input System only; no legacy Input calls.
    /// </summary>
    public sealed class DevToggles : MonoBehaviour
    {
        [Tooltip("Objects to show/hide when the toggle binding is pressed.")]
        public GameObject[] toggleTargets;

#if ENABLE_INPUT_SYSTEM
        [Header("Input System")]
        [Tooltip("Rebindable path. Examples: <Keyboard>/f2, <Keyboard>/backquote, <Gamepad>/start")]
        public string toggleBinding = "<Keyboard>/f2";

        private InputAction _toggleAction;

        private void Awake()
        {
            // Create a simple button action from the binding path
            _toggleAction = new InputAction(type: InputActionType.Button, binding: toggleBinding);
            _toggleAction.Enable();
        }

        private void OnDestroy()
        {
            _toggleAction?.Disable();
            _toggleAction?.Dispose();
        }

        private void Update()
        {
            if (_toggleAction is null) return;
            // WasPressedThisFrame == performed and transitioned from up→down this frame
            if (_toggleAction.WasPressedThisFrame())
            {
                if (toggleTargets == null) return;
                foreach (var go in toggleTargets)
                {
                    if (!go) continue;
                    go.SetActive(!go.activeSelf);
                }
            }
        }
#else
        // Fallback if project isn't using the new Input System.
        [Header("Legacy (if Active Input Handling = Both or Old)")]
        public KeyCode legacyKey = KeyCode.F2;

        private void Update()
        {
            if (Input.GetKeyDown(legacyKey) && toggleTargets != null)
            {
                foreach (var go in toggleTargets)
                {
                    if (!go) continue;
                    go.SetActive(!go.activeSelf);
                }
            }
        }
#endif
    }
}
