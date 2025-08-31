using UnityEngine;
using UnityEngine.InputSystem;        // <-- requires Core asmdef to reference Unity.InputSystem

namespace VerdantRemains.Core
{
    /// <summary>
    /// Minimal sanity test for your Gameplay.inputactions wrapper.
    /// Logs when you click/press and prints mouse position while moving.
    /// </summary>
    public sealed class InputSmoke : MonoBehaviour, GameplayInput.IGameplayActions
    {
        private GameplayInput _input;
        private GameplayInput.GameplayActions _map;

        private void Awake()
        {
            _input = new GameplayInput();
            _map   = _input.Gameplay;

            // Register callbacks via the generated interface
            _map.AddCallbacks(this);
            _map.Enable();
        }

        private void OnDestroy()
        {
            _map.RemoveCallbacks(this);
            _map.Disable();
            _input.Dispose();
        }

        private void Update()
        {
            // Read mouse position continuously
            Vector2 screen = _map.Point.ReadValue<Vector2>();

            // Only spam the log if the mouse is moving
            if (Mouse.current != null && Mouse.current.delta.IsActuated())
            {
                Debug.Log($"[InputSmoke] Point={screen}");
            }
        }

        // ===== Generated interface callbacks =====
        public void OnPoint(InputAction.CallbackContext ctx) { /* unused; using Update() above */ }

        public void OnConfirm(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            Debug.Log("[InputSmoke] Confirm (LMB)");
        }

        public void OnCancel(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            Debug.Log("[InputSmoke] Cancel (RMB)");
        }

        public void OnRotateCW(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            Debug.Log("[InputSmoke] RotateCW (E)");
        }

        public void OnRotateCCW(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            Debug.Log("[InputSmoke] RotateCCW (Q)");
        }
    }
}
