#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DS = VerdantRemains.Build; // BuildableDataStore, ResourceDataStore

namespace VerdantRemains.Editor
{
    /// <summary>
    /// Validates Buildable/Resource data stores before play.
    /// Adjusted to VerdantRemains.Build schema (id, size, prefabs, cost[]).
    /// </summary>
    [InitializeOnLoad]
    public static class DataStoreAuditor
    {
        const bool ENABLE_PLAYMODE_AUDIT = true;
        static bool _auditRunning;

        static DataStoreAuditor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChange;
        }

        [MenuItem("Verdant Remains/Validate/Data Stores")]
        public static void ValidateMenu()
        {
            RunWhenEditorIdle(() =>
            {
                var errs = CollectErrors();
                Report(errs, manual: true, cancelPlayIfErrors: false);
            });
        }

        private static void OnPlayModeChange(PlayModeStateChange s)
        {
            if (!ENABLE_PLAYMODE_AUDIT) return;
            if (s == PlayModeStateChange.ExitingEditMode)
            {
                RunWhenEditorIdle(() =>
                {
                    var errs = CollectErrors();
                    Report(errs, manual: false, cancelPlayIfErrors: true);
                });
            }
        }

        private static void RunWhenEditorIdle(System.Action action, int maxRetries = 200)
        {
            int tries = 0;
            void Tick()
            {
                if (EditorApplication.isCompiling ||
                    EditorApplication.isUpdating ||
                    EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (tries++ < maxRetries) { EditorApplication.delayCall += Tick; }
                    return;
                }

                if (_auditRunning) { EditorApplication.delayCall += Tick; return; }
                _auditRunning = true;
                try { action?.Invoke(); }
                finally { _auditRunning = false; }
            }
            EditorApplication.delayCall += Tick;
        }

        private static bool Report(List<string> errors, bool manual, bool cancelPlayIfErrors)
        {
            if (errors.Count == 0)
            {
                if (manual) Debug.Log("[Audit] Data Store audit passed.");
                return false;
            }

            var msg = string.Join("\n", errors);
            Debug.LogError($"[Audit] Data Store audit failed:\n{msg}");

            if (cancelPlayIfErrors && EditorApplication.isPlaying)
            {
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
                };
            }

            if (manual)
            {
                EditorUtility.DisplayDialog("Verdant Remains — Audit Failed",
                    "Data Store validation failed.\nSee Console for details.", "OK");
            }
            return true;
        }

        private static List<string> CollectErrors()
        {
            var errs = new List<string>();
            var seenIds = new HashSet<string>();

            var buildableGuids = AssetDatabase.FindAssets("t:BuildableDataStore");
            var resourceGuids  = AssetDatabase.FindAssets("t:ResourceDataStore");

            foreach (var guid in buildableGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var t = AssetDatabase.LoadAssetAtPath<DS.BuildableDataStore>(path);
                if (!t) { errs.Add($"{path}: failed to load BuildableDataStore"); continue; }

                // id present + unique
                if (string.IsNullOrWhiteSpace(t.id)) errs.Add($"{path}: empty id");
                else if (!seenIds.Add($"buildable:{t.id}")) errs.Add($"{path}: duplicate id '{t.id}'");

                // size sane
                if (t.size.x < 1 || t.size.y < 1) errs.Add($"{path}: size must be >= 1x1");

                // prefabs wired
                if (!t.prefabGhost)    errs.Add($"{path}: prefabGhost not assigned");
                if (!t.prefabFrame)    errs.Add($"{path}: prefabFrame not assigned");
                if (!t.prefabFinished) errs.Add($"{path}: prefabFinished not assigned");

                // cost valid (using BuildCost[])
                if (t.cost == null || t.cost.Length == 0)
                    errs.Add($"{path}: empty cost");
                else if (t.cost.Any(c => c.amount <= 0))
                    errs.Add($"{path}: invalid cost (non-positive amount)");
            }

            foreach (var guid in resourceGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var r = AssetDatabase.LoadAssetAtPath<DS.ResourceDataStore>(path);
                if (!r) { errs.Add($"{path}: failed to load ResourceDataStore"); continue; }

                // uniqueness by enum + path is usually enough; if you add ids later, check here
                var key = $"resource:{r.material}";
                if (!seenIds.Add(key)) errs.Add($"{path}: duplicate MaterialType '{r.material}'");
                if (string.IsNullOrWhiteSpace(r.displayName))
                    errs.Add($"{path}: displayName empty");
            }

            return errs;
        }
    }
}
#endif
