using UnityEngine;

namespace VerdantRemains.Core
{
    /// <summary>
    /// Minimal sanity checks that avoid compile-time dependencies from Core → other assemblies.
    /// </summary>
    public sealed class ProjectBootstrap : MonoBehaviour
    {
        [Header("Optional: drag any BuildableTemplate asset here")]
        [SerializeField] private Object optionalBuildableTemplateAsset;

        private void Awake()
        {
            Debug.Log("[Bootstrap] Verdant Remains starting…");

#if ENABLE_INPUT_SYSTEM
            Debug.Log("[Bootstrap] Input System: ENABLED");
#else
            Debug.LogWarning("[Bootstrap] Input System NOT enabled (Edit → Player → Active Input Handling).");
#endif

            // Soft probes via reflection (no direct references from Core).
            var gridType = System.Type.GetType("VerdantRemains.Grid.GridManager, VerdantRemains.Grid");
            Debug.Log(gridType != null
                ? "[Bootstrap] Grid assembly detected."
                : "[Bootstrap] Grid assembly not detected yet (ok before Phase 1).");

            var narrativeType = System.Type.GetType("VerdantRemains.Narrative.FeedController, VerdantRemains.Narrative");
            Debug.Log(narrativeType != null
                ? "[Bootstrap] Narrative assembly detected."
                : "[Bootstrap] Narrative assembly not found (fine for Phase 0).");

            if (EventBus.BuildPlaced == null)
                Debug.Log("[Bootstrap] EventBus available. No listeners yet (expected).");

            if (optionalBuildableTemplateAsset)
                Debug.Log($"[Bootstrap] Buildable template asset linked: {optionalBuildableTemplateAsset.name}");
        }
    }
}
