#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VerdantRemains.Build;
using VerdantRemains.Grid;
using VerdantRemains.Core;

namespace VerdantRemains.Editor
{
    /// <summary>
    /// One-click generator for ghost materials, simple cube prefabs, and example data stores.
    /// Use for Phase 0/1 testing.
    /// </summary>
    public static class CreateExampleData
    {
        private const string MAT_DIR   = "Assets/Materials/Builder";
        private const string PREF_DIR  = "Assets/Prefabs/Builder";
        private const string RES_DIR   = "Assets/Resources/Buildables";
        private const string RES_DATA  = "Assets/Resources/ResourcesData";

        [MenuItem("Verdant Remains/Generate Example Builder Data")]
        public static void Generate()
        {
            EnsureFolders(MAT_DIR, PREF_DIR, RES_DIR, RES_DATA);

            // Materials
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var ghostValid   = MakeMaterial($"{MAT_DIR}/Ghost_Valid.mat",   lit, new Color(0.25f, 1f, 0.25f, VRSettings.GHOST_VALID_ALPHA),   true);
            var ghostInvalid = MakeMaterial($"{MAT_DIR}/Ghost_Invalid.mat", lit, new Color(1f, 0.25f, 0.25f, VRSettings.GHOST_INVALID_ALPHA), true);

            // Prefabs (simple cubes)
            var ghostGO   = MakeCube("Ghost_Cube",   ghostValid,   true);
            var frameGO   = MakeCube("Frame_Cube",   MakeMaterial($"{MAT_DIR}/Frame.mat", lit, new Color(0.8f, 0.8f, 0.6f, 1f), false), false);
            var finishedGO= MakeCube("Finished_Cube",MakeMaterial($"{MAT_DIR}/Finished.mat", lit, new Color(0.7f, 0.7f, 0.7f, 1f), false), false);

            var ghostPF   = SaveAsPrefab($"{PREF_DIR}/Ghost_Cube.prefab", ghostGO);
            var framePF   = SaveAsPrefab($"{PREF_DIR}/Frame_Cube.prefab", frameGO);
            var finishedPF= SaveAsPrefab($"{PREF_DIR}/Finished_Cube.prefab", finishedGO);

            Object.DestroyImmediate(ghostGO);
            Object.DestroyImmediate(frameGO);
            Object.DestroyImmediate(finishedGO);

            // Resource Data (Wood, Steel)
            var wood  = ScriptableObject.CreateInstance<ResourceDataStore>();
            wood.material = MaterialType.Wood;  wood.displayName = "Wood";
            AssetDatabase.CreateAsset(wood,  $"{RES_DATA}/Wood.asset");

            var steel = ScriptableObject.CreateInstance<ResourceDataStore>();
            steel.material = MaterialType.Steel; steel.displayName = "Steel";
            AssetDatabase.CreateAsset(steel, $"{RES_DATA}/Steel.asset");

            // Buildable Template (Test Wall 1x1 on Wall layer)
            var wall = ScriptableObject.CreateInstance<BuildableDataStore>();
            wall.id         = "test_wall_1x1";
            wall.layer      = GridLayer.Wall;
            wall.size       = new Vector2Int(1, 1);
            wall.rotation   = RotationType.Cardinal;
            wall.category   = BuildCategory.Structure;
            wall.tags       = BuildTag.Wall;
            wall.prefabGhost    = ghostPF;
            wall.prefabFrame    = framePF;
            wall.prefabFinished = finishedPF;
            wall.cost = new BuildCost[]
            {
                new BuildCost { material = MaterialType.Wood, amount = 5 }
            };
            AssetDatabase.CreateAsset(wall, $"{RES_DIR}/TestWall.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CreateExampleData] Generated ghost materials, cube prefabs, ResourceData (Wood/Steel), and Buildable TestWall.");
        }

        private static void EnsureFolders(params string[] paths)
        {
            foreach (var p in paths)
            {
                if (!AssetDatabase.IsValidFolder(p))
                {
                    var parent = System.IO.Path.GetDirectoryName(p).Replace("\\", "/");
                    var name = System.IO.Path.GetFileName(p);
                    if (!AssetDatabase.IsValidFolder(parent))
                    {
                        // recursively ensure parent
                        var parts = parent.Split('/');
                        string acc = parts[0];
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var next = acc + "/" + parts[i];
                            if (!AssetDatabase.IsValidFolder(next))
                                AssetDatabase.CreateFolder(acc, parts[i]);
                            acc = next;
                        }
                    }
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        private static Material MakeMaterial(string path, Shader shader, Color color, bool transparent)
        {
            var mat = new Material(shader);
            mat.SetColor("_BaseColor", color);
            if (transparent)
            {
                mat.SetFloat("_Surface", 1);       // Transparent
                mat.SetFloat("_ZWrite", 0);        // No Z-write for ghosts
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
            }
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static GameObject MakeCube(string name, Material mat, bool ghost)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            go.transform.localScale = Vector3.one * 1f; // 1×1×1 cell
            if (ghost)
            {
                // Disable collider for ghost
                var col = go.GetComponent<Collider>();
                if (col) Object.DestroyImmediate(col);
            }
            return go;
        }

        private static GameObject SaveAsPrefab(string path, GameObject go)
        {
            return PrefabUtility.SaveAsPrefabAsset(go, path);
        }
    }
}
#endif
