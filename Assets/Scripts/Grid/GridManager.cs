using UnityEngine;

namespace VerdantRemains.Grid
{
    public enum GridLayer { Floor = 0, Wall = 1 }

    /// <summary>Single grid cell with layered occupancy.</summary>
    public class GridCell
    {
        public int x, y;
        public bool occupiedFloor;
        public bool occupiedWall;

        /// <summary>Any-occupancy view (for heatmaps or quick checks).</summary>
        public bool occupied => occupiedFloor || occupiedWall;

        public GridCell(int x, int y) { this.x = x; this.y = y; }
    }

    /// <summary>
    /// Core grid truth (no editor-only code). Initializes in Edit & Play so tooling can query it.
    /// World↔cell conversion assumes XZ plane with Y up.
    /// </summary>
    [ExecuteAlways]
    public sealed class GridManager : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [Min(1)] public int width = 128;
        [Min(1)] public int height = 128;

        [Tooltip("Reserved for later chunked updates; not used yet in Phase 1.")]
        [Min(1)] public int chunkSize = 32;

        [Header("World Mapping")]
        [Tooltip("Bottom-left (min XZ) world position corresponding to cell (0,0).")]
        public Vector3 origin = Vector3.zero;

        [Tooltip("World units per cell (1.0 = 1m per tile).")]
        [Min(0.0001f)] public float cellSize = 1f;

        private GridCell[,] _cells;

        public bool IsInitialized => _cells != null && _cells.Length == width * height;

        #region Unity
        private void OnEnable()
        {
            if (!IsInitialized) Init(width, height, chunkSize);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // Keep cells in sync when dims are edited in the Inspector.
                Init(width, height, chunkSize);
            }
        }
        #endregion

        #region Init
        public void Init(int w, int h, int cSize)
        {
            width = Mathf.Max(1, w);
            height = Mathf.Max(1, h);
            chunkSize = Mathf.Max(1, cSize);

            _cells = new GridCell[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _cells[x, y] = new GridCell(x, y);

            if (Application.isPlaying)
                Debug.Log($"[GridManager] Init {width}x{height}, chunkSize={chunkSize}, cellSize={cellSize}, origin={origin}");
        }
        #endregion

        #region Queries
        public bool InBounds(int x, int y) => (x >= 0 && y >= 0 && x < width && y < height);

        /// <summary>Safe accessor: returns null if uninitialized or out-of-bounds.</summary>
        public GridCell GetCell(int x, int y)
        {
            if (_cells == null) return null;
            if (!InBounds(x, y)) return null;
            return _cells[x, y];
        }

        /// <summary>Treat out-of-bounds as blocked (true) for validators.</summary>
        public bool IsOccupied(int x, int y, GridLayer layer)
        {
            var c = GetCell(x, y);
            if (c == null) return true; // OOB => blocked
            return layer == GridLayer.Floor ? c.occupiedFloor : c.occupiedWall;
        }

        public bool IsAnyOccupied(int x, int y)
        {
            var c = GetCell(x, y);
            if (c == null) return true; // OOB => blocked
            return c.occupied;
        }
        #endregion

        #region Mutations
        public void SetOccupied(int x, int y, GridLayer layer, bool value)
        {
            var c = GetCell(x, y);
            if (c == null) return;
            if (layer == GridLayer.Floor) c.occupiedFloor = value;
            else                           c.occupiedWall  = value;
        }

        /// <summary>Marks a rectangular footprint occupied/free on the given layer.</summary>
        public void SetOccupiedRect(int startX, int startY, int sizeX, int sizeY, GridLayer layer, bool value)
        {
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    SetOccupied(startX + x, startY + y, layer, value);
        }
        #endregion

        #region World <-> Cell
        /// <summary>Clamps a cell coordinate to grid bounds.</summary>
        public Vector2Int ClampToBounds(Vector2Int c)
        {
            c.x = Mathf.Clamp(c.x, 0, width - 1);
            c.y = Mathf.Clamp(c.y, 0, height - 1);
            return c;
        }

        /// <summary>Converts a world point (XZ plane) to a cell coordinate.</summary>
        public Vector2Int WorldToCell(Vector3 world)
        {
            float localX = (world.x - origin.x) / cellSize;
            float localY = (world.z - origin.z) / cellSize;
            return new Vector2Int(Mathf.FloorToInt(localX), Mathf.FloorToInt(localY));
        }

        /// <summary>Converts a cell coordinate to the world position of its bottom-left corner.</summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(
                origin.x + cell.x * cellSize,
                origin.y,
                origin.z + cell.y * cellSize
            );
        }

        /// <summary>Returns the world center of a cell (useful for placing prefabs).</summary>
        public Vector3 CellCenterWorld(Vector2Int cell)
        {
            return new Vector3(
                origin.x + (cell.x + 0.5f) * cellSize,
                origin.y,
                origin.z + (cell.y + 0.5f) * cellSize
            );
        }
        #endregion
    }
}
