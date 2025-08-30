using System;
using UnityEngine;

namespace VerdantRemains.Narrative
{
    /// <summary>
    /// Optional narrative hooks (thin layer above EventBus).
    /// You can raise these from systems that don't want to reference Core directly.
    /// </summary>
    public static class NarrativeEvents
    {
        // Basic hooks — id only (keeps parity with original draft)
        public static Action<string> OnBuildPlaced;   // buildableId
        public static Action<string> OnBuildRemoved;  // buildableId

        // Extended hooks (cell-aware) if needed by the feed
        public static Action<string, Vector2Int> OnBuildPlacedAt;
        public static Action<string, Vector2Int> OnBuildCompletedAt;

        /// <summary>Helpers to raise both classic and extended events.</summary>
        public static void RaisePlaced(string id, Vector2Int cell)
        {
            OnBuildPlaced?.Invoke(id);
            OnBuildPlacedAt?.Invoke(id, cell);
        }

        public static void RaiseCompleted(string id, Vector2Int cell)
        {
            OnBuildCompletedAt?.Invoke(id, cell);
        }

        public static void RaiseRemoved(string id)
        {
            OnBuildRemoved?.Invoke(id);
        }
    }
}
