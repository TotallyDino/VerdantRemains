using UnityEngine;

namespace VerdantRemains.Core
{
    public static class IsoRay
    {
        /// <summary>
        /// Raycast from screen to the ground plane (y = planeY).
        /// Returns true if it hits; outputs the world point in 'world'.
        /// </summary>
        public static bool TryGetGroundPoint(Camera cam, Vector2 screen, float planeY, out Vector3 world)
        {
            world = default;
            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(screen);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

            if (plane.Raycast(ray, out float t))
            {
                world = ray.GetPoint(t);
                return true;
            }
            return false;
        }
    }
}
