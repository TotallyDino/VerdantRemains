using System;
using UnityEngine;

namespace VerdantRemains.Core
{
    public static class EventBus
    {
        public static Action<string, Vector2Int> BuildPlaced;
        public static Action<string, float>      BuildProgressed;
        public static Action<string, Vector2Int> BuildCompleted;

        public static void Reset()
        {
            BuildPlaced     = null;
            BuildProgressed = null;
            BuildCompleted  = null;
        }
    }
}
