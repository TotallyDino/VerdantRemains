using UnityEngine;

namespace VerdantRemains.Core
{
    public static class Mathx
    {
        /// <summary>Convert world XZ to grid cell (floor to int coords).</summary>
        public static Vector2Int WorldToCell(Vector3 world, Vector3 origin, float cellSize)
        {
            float lx = (world.x - origin.x) / cellSize;
            float ly = (world.z - origin.z) / cellSize;
            return new Vector2Int(Mathf.FloorToInt(lx), Mathf.FloorToInt(ly));
        }

        /// <summary>Bottom-left corner of a cell in world space.</summary>
        public static Vector3 CellToWorld(Vector2Int cell, Vector3 origin, float cellSize)
        {
            return new Vector3(
                origin.x + cell.x * cellSize,
                origin.y,
                origin.z + cell.y * cellSize
            );
        }

        /// <summary>Center of a cell in world space (great for prefab placement).</summary>
        public static Vector3 CellCenter(Vector2Int cell, Vector3 origin, float cellSize)
        {
            return new Vector3(
                origin.x + (cell.x + 0.5f) * cellSize,
                origin.y,
                origin.z + (cell.y + 0.5f) * cellSize
            );
        }

        /// <summary>Clamp a cell coordinate to grid bounds.</summary>
        public static Vector2Int ClampCell(Vector2Int cell, int width, int height)
        {
            cell.x = Mathf.Clamp(cell.x, 0, width - 1);
            cell.y = Mathf.Clamp(cell.y, 0, height - 1);
            return cell;
        }
    }
}
