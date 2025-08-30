#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VerdantRemains.Grid;

namespace VerdantRemains.Editor
{
    public static class ManualGridValidator
    {
        [MenuItem("Verdant Remains/Validate/Grid (Manual)")]
        public static void ValidateGrid()
        {
            var grid = Object.FindObjectOfType<GridManager>();
            if (!grid) { Debug.LogError("[GridValidator] No GridManager in scene."); return; }
            if (!grid.IsInitialized) { Debug.LogError("[GridValidator] Grid not initialized."); return; }

            // sample random cells for null checks
            int nulls = 0;
            for (int i = 0; i < 10; i++)
            {
                int x = Random.Range(0, grid.width);
                int y = Random.Range(0, grid.height);
                if (grid.GetCell(x, y) == null) nulls++;
            }
            if (nulls > 0) { Debug.LogError($"[GridValidator] Found null cells ({nulls}/10)."); return; }

            // quick occupancy count
            int occ = 0;
            for (int y = 0; y < grid.height; y++)
                for (int x = 0; x < grid.width; x++)
                    if (grid.GetCell(x, y).occupied) occ++;

            Debug.Log($"[GridValidator] PASS. Size={grid.width}x{grid.height}, Occupied={occ}.");
        }
    }
}
#endif
