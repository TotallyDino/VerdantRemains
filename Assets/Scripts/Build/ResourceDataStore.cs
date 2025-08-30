using UnityEngine;

namespace VerdantRemains.Build
{
    /// <summary>Single source of truth for resource/material kinds used in build costs.</summary>
    public enum MaterialType
    {
        Wood   = 0,
        Stone  = 1,
        Steel  = 2,
        Plastic= 3,
        Cloth  = 4
    }

    /// <summary>Optional: data about a resource type (for UI, icons, stacking, etc.).</summary>
    [CreateAssetMenu(menuName = "VerdantRemains/Resource Data", fileName = "ResourceData")]
    public sealed class ResourceDataStore : ScriptableObject
    {
        public MaterialType material;
        public string displayName;
        public Sprite icon;
        [Min(1)] public int maxStack = 100;
        [Min(0)] public float massPerUnit = 1f;
    }
}
