using UnityEngine;
using VerdantRemains.Grid; // GridLayer

namespace VerdantRemains.Build
{
    /// <summary>Allowed rotations for a buildable at MVP.</summary>
    public enum RotationType
    {
        None,           // no rotation allowed
        Cardinal,       // 0/90/180/270
        Any             // free (later)
    }

    /// <summary>Simple content tag buckets (expand later).</summary>
    [System.Flags]
    public enum BuildTag
    {
        None   = 0,
        Wall   = 1 << 0,
        Floor  = 1 << 1,
        Light  = 1 << 2,
        Door   = 1 << 3
    }

    public enum BuildCategory
    {
        Structure,
        Furniture,
        Utility
    }

    [System.Serializable]
    public struct BuildCost
    {
        public MaterialType material;
        [Min(0)] public int amount;
    }

    /// <summary>
    /// Data-driven template for a placeable object.
    /// Ghost/Frame/Finished are separate prefabs to support the pipeline.
    /// </summary>
    [CreateAssetMenu(menuName = "VerdantRemains/Buildable Template", fileName = "Buildable")]
    public sealed class BuildableDataStore : ScriptableObject
    {
        [Header("Identity")]
        public string id = "buildable_id";

        [Header("Placement")]
        public GridLayer layer = GridLayer.Wall;
        public Vector2Int size = new Vector2Int(1, 1);
        public RotationType rotation = RotationType.Cardinal;
        public BuildCategory category = BuildCategory.Structure;
        public BuildTag tags = BuildTag.Wall;

        [Header("Prefabs")]
        public GameObject prefabGhost;
        public GameObject prefabFrame;
        public GameObject prefabFinished;

        [Header("Costs")]
        public BuildCost[] cost;

        /// <summary>Convenience: true if any cost > 0.</summary>
        public bool HasCost
        {
            get
            {
                if (cost == null) return false;
                for (int i = 0; i < cost.Length; i++)
                    if (cost[i].amount > 0) return true;
                return false;
            }
        }
    }
}
