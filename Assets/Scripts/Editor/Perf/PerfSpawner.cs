// Assets/Scripts/Editor/Perf/PerfSpawner.cs
// Active-scene spawner with prefab GUID persistence, safe Undo, and grid spawning.
// Menu: Verdant ▶ Perf ▶ ...

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VerdantRemains.EditorTools.Perf
{
    public static class PerfSpawner
    {
        private const string MenuRoot     = "Verdant/Perf/";
        private const string PrefKey      = "VerdantRemains.PerfSpawner.PrefabGUID";
        private const string PerfRootName = "__Perf";

        // ---- Grid defaults ----
        private const int   DefaultCount   = 300;
        private const float DefaultSpacing = 2f;

        /// <summary>Spawn the saved prefab (or an empty GameObject if none is set) into the active scene.</summary>
        [MenuItem(MenuRoot + "Spawn", priority = 0)]
        public static void Spawn()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[PerfSpawner] Unity is compiling; aborting spawn.");
                return;
            }

            var prefab = LoadSavedPrefab();
            if (prefab == null)
            {
                Debug.LogWarning("[PerfSpawner] No prefab set. Spawning empty GameObject.");
                SpawnEmptyInternal("Perf_Spawned");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[PerfSpawner] Active scene is not valid. Aborting.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[PerfSpawner] PrefabUtility.InstantiatePrefab returned null. Is the prefab valid?");
                return;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Spawn Perf Prefab");
            EnsurePerfRoot(scene, out var root);
            Undo.SetTransformParent(instance.transform, root.transform, "Parent Perf Object");

            // Drop near the SceneView pivot for convenience
            var sv = SceneView.lastActiveSceneView;
            instance.transform.position = sv != null ? sv.pivot : Vector3.zero;

            Selection.activeObject = instance;
            Ping(instance);
            Debug.Log($"[PerfSpawner] Spawned '{prefab.name}' into scene '{scene.name}'.");
        }

        /// <summary>Spawn an empty object (does not require a saved prefab) into the active scene.</summary>
        [MenuItem(MenuRoot + "Spawn Empty", priority = 1)]
        public static void SpawnEmpty()
        {
            SpawnEmptyInternal("Perf_Spawned");
        }

        /// <summary>Spawn a grid of up to 300 items of the saved prefab (or empties) around the SceneView pivot.</summary>
        [MenuItem(MenuRoot + "Spawn 300 (Grid)", priority = 5)]
        public static void Spawn300Grid()
        {
            SpawnGrid(DefaultCount, DefaultSpacing);
        }

        /// <summary>
        /// Spawns a centered grid of instances around the SceneView pivot.
        /// Uses saved prefab if available; otherwise spawns empty GameObjects.
        /// </summary>
        public static void SpawnGrid(int count, float spacing)
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("[PerfSpawner] Unity is compiling; aborting grid spawn.");
                return;
            }

            if (count <= 0)
            {
                Debug.LogWarning("[PerfSpawner] Count must be > 0.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[PerfSpawner] Active scene is not valid. Aborting.");
                return;
            }

            var prefab = LoadSavedPrefab(); // may be null
            EnsurePerfRoot(scene, out var root);

            var sv = SceneView.lastActiveSceneView;
            var pivot = (sv != null) ? sv.pivot : Vector3.zero;

            // Compute roughly-square grid dims
            GetGridDims(count, out int cols, out int rows);

            // Center grid around pivot
            var totalW = (cols - 1) * spacing;
            var totalH = (rows - 1) * spacing;
            var origin = new Vector3(pivot.x - totalW * 0.5f, pivot.y, pivot.z - totalH * 0.5f);

            // Group undo for a single Ctrl+Z
            Undo.SetCurrentGroupName("Spawn Perf Grid");
            int group = Undo.GetCurrentGroup();

            int spawned = 0;
            for (int r = 0; r < rows && spawned < count; r++)
            {
                for (int c = 0; c < cols && spawned < count; c++)
                {
                    var pos = origin + new Vector3(c * spacing, 0f, r * spacing);
                    GameObject go;

                    if (prefab != null)
                    {
                        go = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                        if (go == null)
                        {
                            Debug.LogError("[PerfSpawner] InstantiatePrefab returned null mid-grid; aborting.");
                            Undo.CollapseUndoOperations(group);
                            return;
                        }
                        Undo.RegisterCreatedObjectUndo(go, "Spawn Perf Prefab");
                    }
                    else
                    {
                        go = new GameObject($"Perf_Spawned_{spawned:000}");
                        SceneManager.MoveGameObjectToScene(go, scene);
                        Undo.RegisterCreatedObjectUndo(go, "Spawn Empty Perf Object");
                    }

                    Undo.SetTransformParent(go.transform, root.transform, "Parent Perf Object");
                    go.transform.position = pos;

                    spawned++;
                }
            }

            Undo.CollapseUndoOperations(group);

            if (prefab != null)
                Debug.Log($"[PerfSpawner] Spawned grid: {spawned}× '{prefab.name}' at spacing {spacing} in '{scene.name}'.");
            else
                Debug.Log($"[PerfSpawner] Spawned grid: {spawned} empty objects at spacing {spacing} in '{scene.name}'.");

            // Select the parent so the grid is easy to find
            Selection.activeObject = root;
            Ping(root);
        }

        /// <summary>Save the currently selected prefab asset as the spawner target (persists by GUID).</summary>
        [MenuItem(MenuRoot + "Set Prefab From Selection", priority = 20)]
        public static void SetPrefabFromSelection()
        {
            var obj = Selection.activeObject;
            if (obj == null)
            {
                Debug.LogWarning("[PerfSpawner] Nothing selected. Select a prefab asset and try again.");
                return;
            }

            // Accept a prefab asset or a prefab instance (resolve to its asset)
            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj) ?? obj;
            var path = AssetDatabase.GetAssetPath(prefabAsset);

            if (string.IsNullOrEmpty(path) || PrefabUtility.GetPrefabAssetType(prefabAsset) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning("[PerfSpawner] Selection is not a prefab asset. Select a .prefab in the Project window.");
                return;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefabAsset, out string guid, out long _))
            {
                Debug.LogError("[PerfSpawner] Could not get GUID for selected prefab.");
                return;
            }

            EditorPrefs.SetString(PrefKey, guid);
            Debug.Log($"[PerfSpawner] Saved prefab GUID for '{prefabAsset.name}'.");
        }

        /// <summary>Clear the saved prefab GUID.</summary>
        [MenuItem(MenuRoot + "Clear Saved Prefab", priority = 21)]
        public static void ClearSavedPrefab()
        {
            EditorPrefs.DeleteKey(PrefKey);
            Debug.Log("[PerfSpawner] Cleared saved prefab GUID.");
        }

        // --------------------------- Helpers ---------------------------

        private static void SpawnEmptyInternal(string name)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[PerfSpawner] Active scene is not valid. Aborting.");
                return;
            }

            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            Undo.RegisterCreatedObjectUndo(go, "Spawn Empty Perf Object");

            EnsurePerfRoot(scene, out var root);
            Undo.SetTransformParent(go.transform, root.transform, "Parent Perf Object");

            var sv = SceneView.lastActiveSceneView;
            go.transform.position = sv != null ? sv.pivot : Vector3.zero;

            Selection.activeObject = go;
            Ping(go);
            Debug.Log($"[PerfSpawner] Spawned empty object into scene '{scene.name}'.");
        }

        private static GameObject LoadSavedPrefab()
        {
            var guid = EditorPrefs.GetString(PrefKey, string.Empty);
            if (string.IsNullOrEmpty(guid))
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[PerfSpawner] Saved prefab GUID no longer resolves to an asset path.");
                return null;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                Debug.LogWarning($"[PerfSpawner] Could not load prefab at path '{path}'. It may have been moved or deleted.");

            return prefab;
        }

        private static void EnsurePerfRoot(Scene scene, out GameObject root)
        {
            root = FindInScene(scene, PerfRootName);
            if (root != null) return;

            root = new GameObject(PerfRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            Undo.RegisterCreatedObjectUndo(root, "Create Perf Root");
        }

        private static GameObject FindInScene(Scene scene, string name)
        {
            if (!scene.IsValid()) return null;
            var roots = scene.GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r.name.Equals(name, StringComparison.Ordinal))
                    return r;
            }
            return null;
        }

        private static void GetGridDims(int count, out int cols, out int rows)
        {
            // Aim for a roughly square grid (cols >= rows)
            var root = Mathf.CeilToInt(Mathf.Sqrt(count));
            cols = root;
            rows = Mathf.CeilToInt((float)count / cols);
        }

        private static void Ping(UnityEngine.Object o) => EditorGUIUtility.PingObject(o);
    }
}
#endif
