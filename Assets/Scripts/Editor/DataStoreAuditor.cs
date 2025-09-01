// Assets/Scripts/Editor/DataStoreAuditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VerdantRemains.EditorTools
{
    public static class DataStoreAuditor
    {
        [MenuItem("Verdant Remains/Audit/Data Stores")]
        public static void AuditAll()
        {
            var issues = new List<string>();
            AuditResources(issues);
            AuditBuildables(issues);

            if (issues.Count == 0)
            {
                Debug.Log("[Audit] Data stores: ✔ No issues found.");
                return;
            }

            Debug.LogWarning($"[Audit] Found {issues.Count} issue(s):\n" + string.Join("\n", issues));
        }

        // --- Resources --------------------------------------------------------

        private static void AuditResources(List<string> issues)
        {
            var guids = AssetDatabase.FindAssets("t:ResourceDataStore");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var res = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (!res)
                {
                    issues.Add($"[Resource] Could not load at path: {path}");
                    continue;
                }

                // Try to read displayName (falls back to asset.name)
                string displayName = ReadMember<string>(res, "displayName", "DisplayName");
                if (string.IsNullOrWhiteSpace(displayName))
                    issues.Add($"[Resource] Missing displayName: {path}");

                // Use explicit id if present, else asset name
                string id = ReadMember<string>(res, "id", "Id");
                if (string.IsNullOrWhiteSpace(id)) id = res.name;

                if (!ids.Add(id))
                    issues.Add($"[Resource] Duplicate id/name '{id}': {path}");
            }
        }

        // --- Buildables -------------------------------------------------------

        private static void AuditBuildables(List<string> issues)
        {
            var guids = AssetDatabase.FindAssets("t:BuildableDataStore");
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var b = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (!b)
                {
                    issues.Add($"[Buildable] Could not load at path: {path}");
                    continue;
                }

                // id
                string id = ReadMember<string>(b, "id", "Id");
                if (string.IsNullOrWhiteSpace(id))
                    issues.Add($"[Buildable] Missing id: {path}");
                else if (!ids.Add(id))
                    issues.Add($"[Buildable] Duplicate id '{id}': {path}");

                // size (Vector2Int or Vector2)
                int sizeX = 0, sizeY = 0;
                var sizeObj = ReadMember<object>(b, "size", "Size");
                if (sizeObj != null)
                {
                    var t = sizeObj.GetType();
                    sizeX = ReadFieldOrProp<int>(sizeObj, t, "x", "X");
                    sizeY = ReadFieldOrProp<int>(sizeObj, t, "y", "Y");
                }
                if (sizeX < 1 || sizeY < 1)
                    issues.Add($"[Buildable] Invalid size {sizeX}x{sizeY}: {path}");

                // prefabs
                var prefabGhost    = ReadMember<UnityEngine.Object>(b, "prefabGhost", "PrefabGhost");
                var prefabFinished = ReadMember<UnityEngine.Object>(b, "prefabFinished", "PrefabFinished");
                if (!prefabGhost)    issues.Add($"[Buildable] prefabGhost missing: {path}");
                if (!prefabFinished) issues.Add($"[Buildable] prefabFinished missing: {path}");

                // costs (robust via SerializedObject; supports BuildCost/Cost with arbitrary field names)
                bool hasAnyPositive = false;
                var so = new SerializedObject(b);
                var costProp = FindCostsArray(so);
                if (costProp != null && costProp.isArray)
                {
                    for (int i = 0; i < costProp.arraySize; i++)
                    {
                        var elem = costProp.GetArrayElementAtIndex(i);
                        var (resourceAssigned, amountValue) = ReadCostElement(elem);

                        if (!resourceAssigned)
                            issues.Add($"[Buildable] Cost entry missing Resource reference: {path} [index {i}]");
                        if (amountValue <= 0)
                            issues.Add($"[Buildable] Non-positive cost amount ({amountValue}): {path} [index {i}]");
                        if (resourceAssigned && amountValue > 0)
                            hasAnyPositive = true;
                    }
                }
                else
                {
                    issues.Add($"[Buildable] No costs array found (expected 'cost'/'costs'): {path}");
                }

                if (!hasAnyPositive)
                    issues.Add($"[Buildable] Has no positive cost entries: {path}");
            }
        }

        // Find a SerializedProperty array that likely holds build costs
        private static SerializedProperty FindCostsArray(SerializedObject so)
        {
            // Try common names first
            foreach (var name in new[] { "cost", "costs", "buildCosts", "buildCost", "Cost", "Costs" })
            {
                var p = so.FindProperty(name);
                if (p != null && p.isArray) return p;
            }
            // Fallback: scan for any array whose element is a managed reference/struct with an int and an object reference
            var iterator = so.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.isArray && iterator.propertyType == SerializedPropertyType.Generic)
                        return iterator; // best-effort fallback
                } while (iterator.NextVisible(false));
            }
            return null;
        }

        // Read one cost element generically:
        // Valid if it has EITHER:
        //   - an object reference to ResourceDataStore
        //   - OR an enum named like "MaterialType" (or a field/property name containing "material")
        // AND a positive integer amount.
        private static (bool resourceAssigned, int amountValue) ReadCostElement(SerializedProperty elem)
        {
            bool resourceAssigned = false; // true if ResourceDataStore ref OR MaterialType enum present
            int amountValue = 0;

            var copy = elem.Copy();
            var end = elem.GetEndProperty();

            while (copy.NextVisible(true) && !SerializedProperty.EqualContents(copy, end))
            {
                switch (copy.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        var obj = copy.objectReferenceValue;
                        if (obj != null && obj.GetType().Name.Contains("ResourceDataStore"))
                            resourceAssigned = true;
                        break;

                    case SerializedPropertyType.Enum:
                        // Accept enums that are clearly material-ish
                        // Unity's SerializedProperty doesn't expose the enum System.Type directly,
                        // but copy.type often contains the enum type name, and copy.name/displayName help too.
                        var typeHint = copy.type;                // e.g., "Enum"
                        var nameHint = copy.name ?? string.Empty;
                        var labelHint = copy.displayName ?? string.Empty;

                        // Be permissive: if any hint suggests "Material", treat as valid
                        if (nameHint.IndexOf("material", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            labelHint.IndexOf("material", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            typeHint.IndexOf("MaterialType", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            resourceAssigned = true; // Schema B: MaterialType present
                        }
                        break;

                    case SerializedPropertyType.Integer:
                        // First integer encountered is treated as amount (common pattern)
                        if (amountValue == 0)
                            amountValue = copy.intValue;
                        break;
                }
            }

            return (resourceAssigned, amountValue);
        }


        // --- Reflection helpers ----------------------------------------------

        private static T ReadMember<T>(object obj, params string[] names)
        {
            if (obj == null) return default;
            var type = obj.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var name in names)
            {
                var p = type.GetProperty(name, flags);
                if (p != null && typeof(T).IsAssignableFrom(p.PropertyType))
                    return (T)p.GetValue(obj);

                var f = type.GetField(name, flags);
                if (f != null && typeof(T).IsAssignableFrom(f.FieldType))
                    return (T)f.GetValue(obj);
            }
            return default;
        }

        private static T ReadFieldOrProp<T>(object obj, Type type, params string[] names)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var name in names)
            {
                var p = type.GetProperty(name, flags);
                if (p != null && p.PropertyType == typeof(T)) return (T)p.GetValue(obj);
                var f = type.GetField(name, flags);
                if (f != null && f.FieldType == typeof(T)) return (T)f.GetValue(obj);
            }
            return default;
        }
    }
}
#endif
