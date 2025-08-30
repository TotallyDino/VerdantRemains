using UnityEngine;

namespace VerdantRemains.Grid
{
    /// <summary>
    /// Draws a filled heatmap over the grid (Scene view). GREEN = free, RED = any occupied.
    /// Now respects origin & cellSize.
    /// </summary>
    [ExecuteAlways]
    public sealed class GridHeatmapGizmos : MonoBehaviour
    {
        public GridManager grid;             // auto-finds if left null
        public bool enabledHeatmap = true;
        [Range(0f,1f)] public float fillAlpha = 0.35f;
        public bool showLegend = true;

        void OnValidate() { if (grid == null) grid = FindObjectOfType<GridManager>(); }

        void OnDrawGizmos()
        {
            if (!enabledHeatmap) return;
            if (grid == null) grid = FindObjectOfType<GridManager>();
            if (grid == null || !grid.IsInitialized)
            {
#if UNITY_EDITOR
                if (showLegend)
                {
                    UnityEditor.Handles.color = Color.white;
                    UnityEditor.Handles.Label(transform.position + new Vector3(0, 0.05f, 0),
                        "[Heatmap] Grid not initialized");
                }
#endif
                return;
            }

            int w = grid.width, h = grid.height;
            var origin = grid.origin;
            var cs = Mathf.Max(0.0001f, grid.cellSize);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                GridCell c = grid.GetCell(x, y);
                if (c == null) continue;

                float v = c.occupied ? 1f : 0f;                   // 0=free, 1=occupied
                Color col = Color.Lerp(Color.green, Color.red, v);
                col.a = fillAlpha;

                Gizmos.color = col;
                var center = new Vector3(origin.x + (x + 0.5f) * cs, origin.y, origin.z + (y + 0.5f) * cs);
                var size   = new Vector3(cs, 0.001f, cs);
                Gizmos.DrawCube(center, size);
            }

#if UNITY_EDITOR
            if (showLegend)
            {
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(transform.position + new Vector3(0, 0.05f, 0),
                    $"Heatmap — GREEN=free, RED=occupied, alpha={fillAlpha:0.00}");
            }
#endif
        }
    }
}
