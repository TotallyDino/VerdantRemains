#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using DS = VerdantRemains.Build;
using VerdantRemains.Grid;

namespace VerdantRemains.Editor
{
    /// <summary>
    /// Convenience helpers to fetch or lazily create data assets during iteration.
    /// Keeps editor-only code out of runtime assemblies.
    /// </summary>
    public static class QuickDataStores
    {
        private const string RES_DIR   = "Assets/Resources/Buildables";
        private const string RES_DATA  = "Assets/Resources/ResourcesData";

        public static DS.ResourceDataStore GetOrCreateResource(DS.MaterialType material, string displayName = null)
        {
            EnsureFolders(RES_DATA);
            string path = $"{RES_DATA}/{material}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<DS.ResourceDataStore>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DS.ResourceDataStore>();
                asset.material   = material;
                asset.displayName= displayName ?? material.ToString();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }
            return asset;
        }

        public static DS.BuildableDataStore GetOrCreateBuildable(string id,
                                                                 GridLayer layer,
                                                                 Vector2Int size,
                                                                 GameObject ghost,
                                                                 GameObject frame,
                                                                 GameObject finished)
        {
            EnsureFolders(RES_DIR);
            string path = $"{RES_DIR}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<DS.BuildableDataStore>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DS.BuildableDataStore>();
                asset.id = id;
                asset.layer = layer;
                asset.size  = size;
                asset.prefabGhost    = ghost;
                asset.prefabFrame    = frame;
                asset.prefabFinished = finished;
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }
            return asset;
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
    }
}
#endif
