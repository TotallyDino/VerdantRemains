using UnityEngine;
using VerdantRemains.Grid;

public sealed class GridQuickSmoke : MonoBehaviour
{
    public GridManager grid;

    void Start()
    {
        if (!grid) grid = FindObjectOfType<GridManager>();
        if (!grid || !grid.IsInitialized) { Debug.LogError("[GridSmoke] Grid not ready"); return; }

        // Pick a safe cell near origin
        int x = 5, y = 5;

        bool inBounds = grid.InBounds(x, y);
        Debug.Log($"[GridSmoke] InBounds({x},{y}) = {inBounds}");

        bool occBefore = grid.IsOccupied(x, y, GridLayer.Wall);
        grid.SetOccupied(x, y, GridLayer.Wall, true);
        bool occAfter = grid.IsOccupied(x, y, GridLayer.Wall);

        Debug.Log($"[GridSmoke] Occupied before={occBefore}, after={occAfter}");
                // Flip Floor separately to prove layers are independent
        bool floorBefore = grid.IsOccupied(x, y, GridLayer.Floor);
        grid.SetOccupied(x, y, GridLayer.Floor, true);
        bool floorAfter  = grid.IsOccupied(x, y, GridLayer.Floor);

        Debug.Log($"[GridSmoke] Floor layer before={floorBefore}, after={floorAfter}; Wall still={grid.IsOccupied(x,y,GridLayer.Wall)}");

    }
}
