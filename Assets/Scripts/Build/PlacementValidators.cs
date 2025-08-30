using UnityEngine;
using VerdantRemains.Grid; // GridManager, GridLayer

namespace VerdantRemains.Build
{
    /// <summary>Static placement checks used by BuildPlacer and editor tools.</summary>
    public static class PlacementValidators
    {
        /// <summary>Bounds + layer occupancy check for a single cell.</summary>
        public static bool CellFree(GridManager grid, int x, int y, GridLayer layer)
        {
            if (grid == null) return false;
            if (!grid.InBounds(x, y)) return false;
            return !grid.IsOccupied(x, y, layer);
        }

        /// <summary>Bounds + layer occupancy check for a rectangular footprint (sizeX×sizeY).</summary>
        public static bool RectFree(GridManager grid, int startX, int startY, int sizeX, int sizeY, GridLayer layer)
        {
            if (grid == null) return false;
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (!CellFree(grid, startX + x, startY + y, layer)) return false;
            return true;
        }

        /// <summary>Top-level convenience for a template footprint.</summary>
        public static bool CanPlace(GridManager grid, Vector2Int cell, BuildableDataStore tpl)
        {
            if (grid == null || tpl == null) return false;
            return RectFree(grid, cell.x, cell.y, Mathf.Max(1, tpl.size.x), Mathf.Max(1, tpl.size.y), tpl.layer);
        }
    }
}
