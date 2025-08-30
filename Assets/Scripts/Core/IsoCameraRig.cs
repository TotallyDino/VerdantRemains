using UnityEngine;

namespace VerdantRemains.Core
{
    /// <summary>
    /// Orthographic zoom controller; pair with PivotMover (parent) for pan/rotate.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class IsoCameraRig : MonoBehaviour
    {
        [Header("Zoom")]
        [Min(0.01f)] public float zoomSpeed = 8f;
        [Min(0.01f)] public float minOrthoSize = 4f;
        [Min(0.01f)] public float maxOrthoSize = 18f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            if (_cam.orthographicSize < minOrthoSize) _cam.orthographicSize = minOrthoSize;
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel"); // OK for MVP; swap to Input System later
            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                float target = Mathf.Clamp(_cam.orthographicSize - scroll * zoomSpeed, minOrthoSize, maxOrthoSize);
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, target, 0.35f);
            }
        }
    }
}
