using UnityEngine;

namespace VerdantRemains.Core
{
    /// <summary>
    /// Moves the camera's parent (pivot) via WASD + Q/E rotate + optional edge pan.
    /// No references to Grid; uses a simple ground plane at Y = planeY.
    /// </summary>
    public sealed class PivotMover : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 12f;
        public bool edgePan = true;
        [Range(0f, 0.2f)] public float edgeThreshold = 0.02f; // % of screen
        public float planeY = 0f;

        [Header("Rotation")]
        public float rotateSpeed = 90f; // deg/sec for key press
        public KeyCode rotateCWKey = KeyCode.E;
        public KeyCode rotateCCWKey = KeyCode.Q;

        [Header("Bounds (optional)")]
        public bool clampToBounds = false;
        public Vector2 minXZ = new Vector2(-100, -100);
        public Vector2 maxXZ = new Vector2( 100,  100);

        private void Update()
        {
            Vector3 move = Vector3.zero;

            // WASD in world-space relative to current yaw
            Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 right   = new Vector3(transform.right.x,   0f, transform.right.z).normalized;

            if (Input.GetKey(KeyCode.W)) move += forward + right;
            if (Input.GetKey(KeyCode.S)) move -= forward + right;
            if (Input.GetKey(KeyCode.D)) move -= forward - right;
            if (Input.GetKey(KeyCode.A)) move += forward - right;

            if (edgePan)
            {
                var mx = (float)Input.mousePosition.x / Screen.width;
                var my = (float)Input.mousePosition.y / Screen.height;
                if (mx <= edgeThreshold)          move += forward - right;
                else if (mx >= 1f - edgeThreshold) move -= forward - right;
                if (my <= edgeThreshold)          move -= forward + right;
                else if (my >= 1f - edgeThreshold) move += forward + right;
            }

            if (move.sqrMagnitude > 0f)
            {
                transform.position += move.normalized * moveSpeed * Time.deltaTime;
            }

            // Rotate around Y
            float yaw = 0f;
            if (Input.GetKey(rotateCWKey))  yaw += 1f;
            if (Input.GetKey(rotateCCWKey)) yaw -= 1f;
            if (Mathf.Abs(yaw) > 0f)
                transform.Rotate(0f, yaw * rotateSpeed * Time.deltaTime, 0f, Space.World);

            // Clamp & snap Y to plane
            var p = transform.position;
            if (clampToBounds)
            {
                p.x = Mathf.Clamp(p.x, minXZ.x, maxXZ.x);
                p.z = Mathf.Clamp(p.z, minXZ.y, maxXZ.y);
            }
            p.y = planeY;
            transform.position = p;
        }
    }
}
