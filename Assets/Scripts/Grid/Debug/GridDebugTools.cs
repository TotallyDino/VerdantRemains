using UnityEngine;

namespace VerdantRemains.Grid
{
    /// <summary>
    /// Handy debug utilities for flipping GridCell states (Scene or Play).
    /// Attach anywhere, use the context menu actions.
    /// </summary>
    [ExecuteAlways]
    public sealed class GridDebugTools : MonoBehaviour
    {
        [Header("Refs")]
        public GridManager grid;                  // auto-found if null

        [Header("Defaults")]
        public bool defaultOccupied = true;       // value used by Fill/Set actions
        public int rectWidth = 8;
        public int rectHeight = 4;
        [Range(0f,1f)] public float randomFillProbability = 0.25f;

        void OnValidate()
        {
            if (grid == null) grid = FindObjectOfType<GridManager>();
        }

        bool EnsureGrid()
        {
            if (grid == null) grid = FindObjectOfType<GridManager>();
            if (grid == null || !grid.IsInitialized)
            {
                Debug.LogWarning("[GridDebugTools] Grid not found or not initialized.");
                return false;
            }
            return true;
        }

        Vector2Int WorldToGrid(Vector3 world) => grid.WorldToCell(world);

        bool InBounds(int x, int y) => grid.InBounds(x, y);

        bool TryGetCell(int x, int y, out GridCell cell)
        {
            cell = null;
            if (!EnsureGrid() || !InBounds(x, y)) return false;
            cell = grid.GetCell(x, y);
            return cell != null;
        }

        // ---------- Single-cell ----------
        [ContextMenu("Cell/Toggle at Transform (Wall)")]
        public void ToggleCellAtTransform()
        {
            if (!EnsureGrid()) return;
            var g = WorldToGrid(transform.position);
            if (TryGetCell(g.x, g.y, out var c))
            {
                bool newVal = !c.occupiedWall;
                grid.SetOccupied(g.x, g.y, GridLayer.Wall, newVal);
                Debug.Log($"[GridDebugTools] Toggle cell ({g.x},{g.y}) Wall -> {newVal}");
            }
            else
            {
                Debug.LogWarning($"[GridDebugTools] OOB at ({g.x},{g.y})");
            }
        }

        [ContextMenu("Cell/Set at Transform (Wall=defaultOccupied)")]
        public void SetCellAtTransform()
        {
            if (!EnsureGrid()) return;
            var g = WorldToGrid(transform.position);
            if (TryGetCell(g.x, g.y, out _))
            {
                grid.SetOccupied(g.x, g.y, GridLayer.Wall, defaultOccupied);
                Debug.Log($"[GridDebugTools] Set cell ({g.x},{g.y}) Wall = {defaultOccupied}");
            }
            else
            {
                Debug.LogWarning($"[GridDebugTools] OOB at ({g.x},{g.y})");
            }
        }

        // ---------- Area ----------
        [ContextMenu("Area/Fill Rect Around Transform (Wall=defaultOccupied)")]
        public void FillRectAroundTransform()
        {
            if (!EnsureGrid()) return;
            var center = WorldToGrid(transform.position);
            int x0 = center.x - rectWidth / 2;
            int y0 = center.y - rectHeight / 2;
            int x1 = x0 + rectWidth - 1;
            int y1 = y0 + rectHeight - 1;

            int count = 0;
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                if (TryGetCell(x, y, out _)) { grid.SetOccupied(x, y, GridLayer.Wall, defaultOccupied); count++; }

            Debug.Log($"[GridDebugTools] FillRect around ({center.x},{center.y}) {rectWidth}x{rectHeight} -> {defaultOccupied} (touched {count} cells)");
        }

        [ContextMenu("Area/Clear All (both layers=false)")]
        public void ClearAll()
        {
            if (!EnsureGrid()) return;
            int count = 0;
            for (int y = 0; y < grid.height; y++)
            for (int x = 0; x < grid.width; x++)
                if (TryGetCell(x, y, out _)) { grid.SetOccupied(x, y, GridLayer.Floor, false); grid.SetOccupied(x, y, GridLayer.Wall, false); count++; }
            Debug.Log($"[GridDebugTools] Cleared occupied on {count} cells (both layers)");
        }

        [ContextMenu("Area/Random Fill (Wall occupied ~ probability)")]
        public void RandomFill()
        {
            if (!EnsureGrid()) return;
            int count = 0, hits = 0;
            var rng = new System.Random(12345); // deterministic

            for (int y = 0; y < grid.height; y++)
            for (int x = 0; x < grid.width; x++)
            {
                if (!TryGetCell(x, y, out _)) continue;
                bool v = rng.NextDouble() < randomFillProbability;
                grid.SetOccupied(x, y, GridLayer.Wall, v);
                count++;
                if (v) hits++;
            }

            Debug.Log($"[GridDebugTools] RandomFill p={randomFillProbability:0.00} -> {hits}/{count} Wall-occupied");
        }

        // ---------- Patterns ----------
        [ContextMenu("Patterns/Mark Row y=10 (Wall=true)")]
        public void MarkRowY10()
        {
            if (!EnsureGrid()) return;
            int y = Mathf.Clamp(10, 0, grid.height - 1);
            int touched = 0;
            for (int x = 0; x < grid.width; x++)
                if (TryGetCell(x, y, out _)) { grid.SetOccupied(x, y, GridLayer.Wall, true); touched++; }
            Debug.Log($"[GridDebugTools] Row y={y} marked Wall occupied ({touched} cells)");
        }

        [ContextMenu("Patterns/Checkerboard (Wall on even parity)")]
        public void Checkerboard()
        {
            if (!EnsureGrid()) return;
            int touched = 0;
            for (int y = 0; y < grid.height; y++)
            for (int x = 0; x < grid.width; x++)
                if (TryGetCell(x, y, out _)) { grid.SetOccupied(x, y, GridLayer.Wall, (((x + y) & 1) == 0)); touched++; }
            Debug.Log($"[GridDebugTools] Checkerboard set on {touched} cells (Wall layer)");
        }
    }
}
