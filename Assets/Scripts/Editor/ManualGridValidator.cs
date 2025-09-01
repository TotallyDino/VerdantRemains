// Assets/Scripts/Editor/ManualGridValidator.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace VerdantRemains.EditorTools
{
    public static class ManualGridValidator
    {
        [MenuItem("Verdant Remains/Debug/Validate Grid (Manual)")]
        public static void Validate()
        {
            var grid = FindGridManager();
            if (!grid)
            {
                Debug.LogWarning("[Grid] Manual validation: GridManager not found in the open scene.");
                return;
            }

            // Try to read width/height/cellSize from either fields or properties (camelCase or PascalCase)
            int width  = GetMemberValue<int>(grid, "Width", "width");
            int height = GetMemberValue<int>(grid, "Height", "height");
            float cell = GetMemberValue<float>(grid, "CellSize", "cellSize");

            bool inBounds = (width > 0 && height > 0 && cell > 0f);
            if (!inBounds)
            {
                Debug.LogWarning($"[Grid] Invalid config: width={width}, height={height}, cellSize={cell}");
            }
            else
            {
                Debug.Log($"[Grid] OK — Size={width}x{height}, Cell={cell}");
            }
        }

        private static VerdantRemains.Grid.GridManager FindGridManager()
        {
#if UNITY_2022_2_OR_NEWER
            var g = UnityEngine.Object.FindFirstObjectByType<VerdantRemains.Grid.GridManager>();
            if (!g) g = UnityEngine.Object.FindAnyObjectByType<VerdantRemains.Grid.GridManager>();
            return g;
#else
            return UnityEngine.Object.FindObjectOfType<VerdantRemains.Grid.GridManager>();
#endif
        }

        private static T GetMemberValue<T>(object obj, params string[] names)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = obj.GetType();

            foreach (var name in names)
            {
                // Property first
                var prop = type.GetProperty(name, flags);
                if (prop != null && prop.PropertyType == typeof(T))
                    return (T)prop.GetValue(obj);

                // Field fallback
                var field = type.GetField(name, flags);
                if (field != null && field.FieldType == typeof(T))
                    return (T)field.GetValue(obj);
            }

            // If not found or wrong type, return default(T)
            return default;
        }
    }
}
#endif
