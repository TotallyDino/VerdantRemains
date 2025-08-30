using UnityEngine;

namespace VerdantRemains.Core
{
    /// <summary>
    /// Global constants / knobs. Avoid enums here unless truly cross-domain.
    /// </summary>
    public static class VRSettings
    {
        // Grid
        public const int   DEFAULT_GRID_WIDTH  = 128;
        public const int   DEFAULT_GRID_HEIGHT = 128;
        public const float DEFAULT_CELL_SIZE   = 1f;

        // Ghost visuals
        public const float GHOST_VALID_ALPHA   = 0.55f;
        public const float GHOST_INVALID_ALPHA = 0.55f;

        // Work loop (tune later)
        public const float WORK_UNITS_PER_SEC  = 1.0f;

        // Input map (names only; actions live in Input System asset)
        public const string ACTION_MAP_GAMEPLAY = "Gameplay";
        public const string ACTION_POINT        = "Point";
        public const string ACTION_CONFIRM      = "Confirm";
        public const string ACTION_CANCEL       = "Cancel";
        public const string ACTION_ROTATE_CW    = "RotateCW";
        public const string ACTION_ROTATE_CCW   = "RotateCCW";
    }
}
