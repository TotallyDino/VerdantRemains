using UnityEngine;

namespace VerdantRemains.Grid
{
    /// <summary>
    /// Draws cell and chunk wireframes for the active GridManager.
    /// Now respects origin & cellSize.
    /// </summary>
    [ExecuteAlways]
    public sealed class GridDebugGizmos : MonoBehaviour
    {
        public GridManager grid;          // drag or auto-find
        public bool drawCells  = true;
        public bool drawChunks = true;
        public bool labelChunkCoords = false;

        [Range(0f, 1f)] public float cellAlpha = 0.07f;
        [Range(0f, 1f)] public float chunkAlpha = 0.45f;

        void OnValidate()
        {
            if (grid == null) grid = FindObjectOfType<GridManager>();
        }

        void OnDrawGizmos()
        {
            if (grid == null) grid = FindObjectOfType<GridManager>();
            if (grid == null || !grid.IsInitialized) return;

            int w = grid.width;
            int h = grid.height;
            int s = Mathf.Max(1, grid.chunkSize);

            var origin = grid.origin;
            var cs = Mathf.Max(0.0001f, grid.cellSize);

            // Draw cells
            if (drawCells)
            {
                Gizmos.color = new Color(1f, 1f, 1f, Mathf.Clamp01(cellAlpha));
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var center = new Vector3(origin.x + (x + 0.5f) * cs, origin.y, origin.z + (y + 0.5f) * cs);
                    var size   = new Vector3(cs, 0f, cs);
                    Gizmos.DrawWireCube(center, size);
                }
            }

            // Draw chunk bounds
            if (drawChunks)
            {
                Gizmos.color = new Color(0f, 1f, 1f, Mathf.Clamp01(chunkAlpha));
                for (int cy = 0; cy < h; cy += s)
                for (int cx = 0; cx < w; cx += s)
                {
                    int cw = Mathf.Min(s, w - cx);
                    int ch = Mathf.Min(s, h - cy);

                    var center = new Vector3(
                        origin.x + (cx + cw / 2f) * cs,
                        origin.y,
                        origin.z + (cy + ch / 2f) * cs);
                    var size   = new Vector3(cw * cs, 0f, ch * cs);

                    Gizmos.DrawWireCube(center, size);

#if UNITY_EDITOR
                    if (labelChunkCoords)
                    {
                        UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.9f);
                        UnityEditor.Handles.Label(center + new Vector3(0, 0.05f, 0), $"({cx},{cy})");
                    }
#endif
                }
            }
        }
    }
}
